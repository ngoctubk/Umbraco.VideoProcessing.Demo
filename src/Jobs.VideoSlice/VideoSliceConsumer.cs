using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using MassTransit;
using MassTransit.Messages;

namespace Jobs.VideoSlice;

public class VideoSliceConsumer(ILogger<VideoSliceConsumer> logger,
                                S3StorageSettings s3StorageSettings,
                                IAmazonS3 amazonS3,
                                IPublishEndpoint publishEndpoint) : IConsumer<VideoProccessed>
{
    public async Task Consume(ConsumeContext<VideoProccessed> context)
    {
        string mediaPath = context.Message.S3Key;

        logger.LogInformation(mediaPath);
        Console.WriteLine($"Video saved key: {mediaPath}");

        // if (mediaPath.StartsWith("/"))
        //     mediaPath = mediaPath[1..];
        // Check if files exists in convention path

        // Download from S3 and save to convention path
        var request = new GetObjectRequest
        {
            BucketName = s3StorageSettings.BucketName,
            Key = mediaPath
        };
        GetObjectResponse response = await amazonS3.GetObjectAsync(request);
        string rootPath = "/home/vscode";
        string fullFilePath = $"{rootPath}{mediaPath}";
        var directoryPath = Path.GetDirectoryName(fullFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        Directory.CreateDirectory(directoryPath);
        Directory.CreateDirectory(Path.Combine(directoryPath, "360"));
        Directory.CreateDirectory(Path.Combine(directoryPath, "480"));
        Directory.CreateDirectory(Path.Combine(directoryPath, "720"));
        Directory.CreateDirectory(Path.Combine(directoryPath, "1080"));

        var fileName = Path.GetFileName(fullFilePath);
        System.Console.WriteLine("fileName: {0}", fileName);

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath) ?? throw new InvalidOperationException("Could not get file name without extension.");
        string m3u8File = Path.Combine(directoryPath, fileNameWithoutExtension + ".m3u8");
        Console.WriteLine($"m3u8File: {m3u8File}");
        var extension = Path.GetExtension(fullFilePath) ?? throw new InvalidOperationException("Could not get extension.");
        Console.WriteLine("extension: {0}", extension);
        var cutFilesName = Path.Combine(directoryPath, fileNameWithoutExtension + "_%03d" + extension);
        Console.WriteLine($"cutFilesName: {cutFilesName}");
        using var fileStream = File.Create(fullFilePath);
        response.ResponseStream.CopyTo(fileStream);

        // Run ffmpeg to slice video into convention path
        var result = await Cli.Wrap("ffmpeg")
                        .WithArguments(args => args
                            .Add("-i").Add(fullFilePath)
                            .Add("-c").Add("copy")
                            .Add("-map").Add("0")
                            .Add("-sc_threshold").Add("0")
                            .Add("-f").Add("segment")
                            .Add("-segment_time").Add("10")
                            .Add("-segment_list").Add(m3u8File)
                            .Add("-segment_list_flags").Add("+live")
                            .Add(cutFilesName)
                        ).ExecuteAsync();

        System.Console.WriteLine("Success: {0}", result.IsSuccess);

        string m3u8Content = File.ReadAllText(m3u8File);
        // m3u8Content = m3u8Content.Replace(extension, ".ts");
        var prefixPath = Path.GetDirectoryName(mediaPath) ?? throw new InvalidOperationException("Could not get prefix directory.");
        
        string m3u8Content360 = Regex.Replace(m3u8Content, @$"(.*)({extension})", Path.Combine(prefixPath, "360", "$1" + ".ts"));
        await File.WriteAllTextAsync(Path.Combine(directoryPath, "360", fileNameWithoutExtension + ".m3u8"), m3u8Content360);
        string m3u8Content480 = Regex.Replace(m3u8Content, @$"(.*)({extension})", Path.Combine(prefixPath, "480", "$1" + ".ts"));
        await File.WriteAllTextAsync(Path.Combine(directoryPath, "480", fileNameWithoutExtension + ".m3u8"), m3u8Content480);
        string m3u8Content720 = Regex.Replace(m3u8Content, @$"(.*)({extension})", Path.Combine(prefixPath, "720", "$1" + ".ts"));
        await File.WriteAllTextAsync(Path.Combine(directoryPath, "720", fileNameWithoutExtension + ".m3u8"), m3u8Content720);
        string m3u8Content1080 = Regex.Replace(m3u8Content, @$"(.*)({extension})", Path.Combine(prefixPath, "1080", "$1" + ".ts"));
        await File.WriteAllTextAsync(Path.Combine(directoryPath, "1080", fileNameWithoutExtension + ".m3u8"), m3u8Content1080);

        // master playlist
        string masterPlaylistContent = File.ReadAllText("MasterPlaylist.m3u8");
        masterPlaylistContent = masterPlaylistContent.Replace("VideoFileS3StoragePath", mediaPath);
        await File.WriteAllTextAsync(Path.Combine(directoryPath, "MasterPlaylist.m3u8"), masterPlaylistContent);
        // Upload each parts to S3 && publish messages to RabbitMQ
        string[] allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        // Filter out the files to exclude
        var filesToProcess = allFiles.Where(file => !fileName.Contains(Path.GetFileName(file)));

        foreach (var file in filesToProcess)
        {
            Console.WriteLine(file); // Or process the file as needed
            string relativeFile = file.Replace(rootPath, "");
            Console.WriteLine(relativeFile);

            using FileStream uploadFileStream = new(file, FileMode.Open, System.IO.FileAccess.Read);
            var uploadRequest = new PutObjectRequest
            {
                BucketName = s3StorageSettings.BucketName,
                Key = relativeFile,
                InputStream = uploadFileStream
            };

            var uploadResponse = await amazonS3.PutObjectAsync(uploadRequest);
            if (relativeFile.EndsWith(extension))
            {
                // Publish message to convert video file
                await publishEndpoint.Publish(new VideoPartUploaded
                {
                    S3Key = relativeFile
                }, context =>
                {
                    context.Durable = true;
                });
            }
        }


        // Publish successful message to RabbitMQ 
        await publishEndpoint.Publish(new VideoCut
        {
            S3Key = mediaPath
        }, context =>
        {
            context.Durable = true;
        });
    }
}
