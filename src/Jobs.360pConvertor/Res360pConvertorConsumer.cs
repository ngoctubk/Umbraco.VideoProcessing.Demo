using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using MassTransit;
using MassTransit.Messages;

namespace Jobs._360pConvertor;

public class Res360pConvertorConsumer(ILogger<Res360pConvertorConsumer> logger,
                                S3StorageSettings s3StorageSettings,
                                IAmazonS3 amazonS3,
                                IPublishEndpoint publishEndpoint) : IConsumer<VideoPartUploaded>
{
    public async Task Consume(ConsumeContext<VideoPartUploaded> context)
    {
        string mediaPath = context.Message.S3Key;

        logger.LogInformation(mediaPath);
        Console.WriteLine($"Video part key: {mediaPath}");

        // Check file exists before downloading
        string rootPath = "/home/vscode";
        string fullFilePath = $"{rootPath}{mediaPath}";
        var directoryPath = Path.GetDirectoryName(fullFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        if (!File.Exists(fullFilePath))
        {
            var request = new GetObjectRequest
            {
                BucketName = s3StorageSettings.BucketName,
                Key = mediaPath
            };
            GetObjectResponse response = await amazonS3.GetObjectAsync(request);

            Directory.CreateDirectory(directoryPath);

            using var fileStream = File.Create(fullFilePath);
            response.ResponseStream.CopyTo(fileStream);
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath) ?? throw new InvalidOperationException("Could not get file name without extension."); ;
        string convertedPath = Path.Combine(directoryPath, "360");
        string fileConvertedPath = Path.Combine(convertedPath, fileNameWithoutExtension + ".ts");

        // Run ffmpeg to convert video into convention path
        var result = await Cli.Wrap("ffmpeg")
                        .WithArguments(args => args
                            .Add("-i").Add(fullFilePath)
                            .Add("-vf").Add("scale=-2:360")
                            .Add("-crf").Add("30")
                            .Add("-c:v").Add("libx264")
                            .Add("-c:a").Add("aac")
                            .Add("-copyts")
                            .Add(fileConvertedPath)
                        ).ExecuteAsync();

        // Upload ts file to S3
        string relativeFile = fileConvertedPath.Replace(rootPath, "");
        Console.WriteLine(relativeFile);
        using FileStream uploadFileStream = new(fileConvertedPath, FileMode.Open, System.IO.FileAccess.Read);
        var uploadRequest = new PutObjectRequest
        {
            BucketName = s3StorageSettings.BucketName,
            Key = relativeFile,
            InputStream = uploadFileStream
        };

        var uploadResponse = await amazonS3.PutObjectAsync(uploadRequest);
    }
}
