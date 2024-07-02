using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.SliceVideo;

public class SliceVideoConsumer(IOptions<CommonSettings> optionCommonSettings,
                                IPublishEndpoint publishEndpoint,
                                S3StorageSettings s3StorageSettings,
                                IAmazonS3 amazonS3) : IConsumer<VideoProccessed>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    private const string EventName = "SliceVideo";
    private const string playlistExtension = ".m3u8";
    public async Task Consume(ConsumeContext<VideoProccessed> context)
    {
        string mediaPath = context.Message.S3Key;

        string rootMediaPath = _commonSettings.MediaPath;
        string fullVideoPath = Path.Join(rootMediaPath, mediaPath);
        string directoryPath = Path.GetDirectoryName(fullVideoPath) ?? throw new InvalidOperationException("Could not get directory.");
        string videoName = Path.GetFileName(fullVideoPath) ?? throw new InvalidOperationException("Could not get file name."); ;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullVideoPath) ?? throw new InvalidOperationException("Could not get file name without extension.");
        string extension = Path.GetExtension(fullVideoPath) ?? throw new InvalidOperationException("Could not get extension.");

        string cutDirectoryPath = Path.Combine(directoryPath, "raw");
        Directory.CreateDirectory(cutDirectoryPath);

        int VideoPartDuration = _commonSettings.VideoPartDuration;

        string m3u8File = Path.Combine(cutDirectoryPath, fileNameWithoutExtension + playlistExtension);
        string cutFilesName = Path.Combine(cutDirectoryPath, fileNameWithoutExtension + "_%04d" + extension);

        await DownloadVideoFromS3(mediaPath, fullVideoPath);

        await Cli.Wrap("ffmpeg")
                        .WithArguments(args => args
                            .Add("-i").Add(fullVideoPath)
                            .Add("-c").Add("copy")
                            .Add("-map").Add("0")
                            .Add("-sc_threshold").Add("0")
                            .Add("-f").Add("segment")
                            .Add("-segment_time").Add(VideoPartDuration)
                            .Add("-segment_list").Add(m3u8File)
                            .Add("-segment_list_flags").Add("+live")
                            .Add(cutFilesName)
                        ).ExecuteAsync();

        string[] allFiles = Directory.GetFiles(cutDirectoryPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in allFiles.OrderBy(f => f))
        {
            string relativeFile = file.Replace(rootMediaPath, "");

            Console.WriteLine(relativeFile);

            await UploadFileToS3(file, relativeFile);

            if (relativeFile.EndsWith(extension))
            {
                await PublishVideoPartUploadedEvent(relativeFile, mediaPath);
            }
            else if (relativeFile.EndsWith(playlistExtension))
            {
                await PublishPlaylistUploadedEvent(relativeFile, mediaPath);
            }
        }

        await PublishVideoCutEvent(mediaPath);
    }

    private async ValueTask UploadFileToS3(string file, string relativeFile)
    {
        using FileStream uploadFileStream = new(file, FileMode.Open, System.IO.FileAccess.Read);
        var uploadRequest = new PutObjectRequest
        {
            BucketName = s3StorageSettings.BucketName,
            Key = relativeFile,
            InputStream = uploadFileStream
        };
        var uploadResponse = await amazonS3.PutObjectAsync(uploadRequest);
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

    private async ValueTask PublishPlaylistUploadedEvent(string playListPath, string mediaPath)
    {
        PlaylistUploaded message = new()
        {
            S3Key = playListPath,
            OriginalFileS3Key = mediaPath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }

    private async ValueTask PublishVideoPartUploadedEvent(string videoPartPath, string mediaPath)
    {
        VideoPartUploaded message = new()
        {
            S3Key = videoPartPath,
            OriginalFileS3Key = mediaPath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }

    private async ValueTask PublishVideoCutEvent(string mediaPath)
    {
        VideoCut message = new()
        {
            S3Key = mediaPath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
