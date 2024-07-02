using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using Jobs.ConvertVideo.Settings;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.ConvertVideo;

public class ConvertVideoPartConsumer(IOptions<CommonSettings> optionCommonSettings,
                                     IPublishEndpoint publishEndpoint,
                                     S3StorageSettings s3StorageSettings,
                                     IAmazonS3 amazonS3) : IConsumer<VideoPartResolutionSelected>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;

    public async Task Consume(ConsumeContext<VideoPartResolutionSelected> context)
    {
        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFilePath;
        string resolution = context.Message.Resolution.ToString();

        string rootMediaPath = _commonSettings.MediaPath;
        string fullVideoPath = Path.Join(rootMediaPath, mediaPath);
        string directoryPath = Path.GetDirectoryName(fullVideoPath) ?? throw new InvalidOperationException("Could not get directory.");
        string videoName = Path.GetFileName(fullVideoPath) ?? throw new InvalidOperationException("Could not get file name."); ;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullVideoPath) ?? throw new InvalidOperationException("Could not get file name without extension.");
        string extension = Path.GetExtension(fullVideoPath) ?? throw new InvalidOperationException("Could not get extension.");

        await DownloadVideoFromS3(mediaPath, fullVideoPath);

        await ConvertVideoPart(context);

        await UploadVideoPartToS3(context);

        await PublishVideoConvertedEvent(context);
    }

    private async ValueTask ConvertVideoPart(ConsumeContext<VideoPartResolutionSelected> context)
    {
        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFilePath;
        string resolution = context.Message.Resolution.ToString();

        string rootMediaPath = _commonSettings.MediaPath;

        string videoPartPath = Path.Join(rootMediaPath, mediaPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaPath) ?? throw new InvalidOperationException("Could not get file name without extension.");

        string fullOriginalFilePath = Path.Join(rootMediaPath, originalFilePath);
        string originalFileDirectoryPath = Path.GetDirectoryName(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        string convertedVideoPartPath = Path.Join(originalFileDirectoryPath, resolution, fileNameWithoutExtension + ".ts");

        await Cli.Wrap("ffmpeg")
                .WithArguments(args => args
                    .Add("-i").Add(videoPartPath)
                    .Add("-vf").Add("scale=-2:360")
                    .Add("-crf").Add("30")
                    .Add("-c:v").Add("libx264")
                    .Add("-c:a").Add("aac")
                    .Add("-copyts")
                    .Add(convertedVideoPartPath)
                ).ExecuteAsync();
    }

    private async ValueTask UploadVideoPartToS3(ConsumeContext<VideoPartResolutionSelected> context)
    {
        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFilePath;
        string resolution = context.Message.Resolution.ToString();

        string rootMediaPath = _commonSettings.MediaPath;

        string fullOriginalFilePath = Path.Join(rootMediaPath, originalFilePath);
        string originalFileDirectoryPath = Path.GetDirectoryName(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get directory.");

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaPath) ?? throw new InvalidOperationException("Could not get file name without extension.");

        string convertedVideoPartPath = Path.Join(originalFileDirectoryPath, resolution, fileNameWithoutExtension + ".ts");
        string convertedVideoPartRelativePath = convertedVideoPartPath.Replace(rootMediaPath, "");

        using FileStream uploadFileStream = new(convertedVideoPartPath, FileMode.Open, System.IO.FileAccess.Read);
        var uploadRequest = new PutObjectRequest
        {
            BucketName = s3StorageSettings.BucketName,
            Key = convertedVideoPartRelativePath,
            InputStream = uploadFileStream
        };
        var uploadResponse = await amazonS3.PutObjectAsync(uploadRequest);
    }

    private async ValueTask PublishVideoConvertedEvent(ConsumeContext<VideoPartResolutionSelected> context)
    {
        string videoPartPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFilePath;
        int resolution = context.Message.Resolution;

        VideoPartConverted message = new()
        {
            S3Key = videoPartPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }

    private async ValueTask DownloadVideoFromS3(string mediaPath, string fullVideoPath)
    {
        if (!File.Exists(fullVideoPath))
        {
            string directoryPath = Path.GetDirectoryName(fullVideoPath) ?? throw new InvalidOperationException("Could not get directory.");
            Directory.CreateDirectory(directoryPath);

            var request = new GetObjectRequest
            {
                BucketName = s3StorageSettings.BucketName,
                Key = mediaPath
            };
            GetObjectResponse response = await amazonS3.GetObjectAsync(request);
            using FileStream fileStream = File.Create(fullVideoPath);
            await response.ResponseStream.CopyToAsync(fileStream);
        }
    }
}
