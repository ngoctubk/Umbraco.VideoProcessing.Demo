﻿using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using Commons;
using Jobs.ConvertVideo.Settings;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.ConvertVideo;

public class VideoPartConvertorService(IOptions<CommonSettings> optionCommonSettings,
                                     IPublishEndpoint publishEndpoint,
                                     S3StorageSettings s3StorageSettings,
                                     MediaProcessingDbContext dbContext,
                                     IAmazonS3 amazonS3)
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    private const string EventName = "VideoPartResolutionSelected";
    private const string TaskName = "VideoPartResolutionConvert";

    public async ValueTask Convert(VideoPartResolutionSelected videoPartResolutionSelected)
    {
        await AddVideoPartResolutionSelectedEvent(videoPartResolutionSelected);

        var processingTaskId = await AddVideoPartResolutionConvertTask(videoPartResolutionSelected);

        string mediaPath = videoPartResolutionSelected.S3Key;

        string rootMediaPath = _commonSettings.MediaPath;
        string fullVideoPath = Path.Join(rootMediaPath, mediaPath);

        await DownloadVideoFromS3(mediaPath, fullVideoPath);

        await ConvertVideoPart(videoPartResolutionSelected);

        await UploadVideoPartToS3(videoPartResolutionSelected);

        await PublishVideoConvertedEvent(videoPartResolutionSelected, processingTaskId);
    }

    private async ValueTask<Guid> AddVideoPartResolutionConvertTask(VideoPartResolutionSelected message)
    {
        if (message.TaskId != null)
        {
            return message.TaskId.Value;
        }
        
        DateTime currentDate = DateTime.Now;
        var processingTask = new ProcessingTask
        {
            VideoPath = message.OriginalFilePath,
            MediaPartPath = message.S3Key,
            Resolution = message.Resolution,
            TaskName = TaskName,
            TaskContent = JsonSerializer.Serialize(message),
            IsDone = false,
            CreatedDate = currentDate,
            ModifiedDate = currentDate
        };
        dbContext.Add(processingTask);
        await dbContext.SaveChangesAsync();

        return processingTask.Id;
    }

    private async ValueTask AddVideoPartResolutionSelectedEvent(VideoPartResolutionSelected message)
    {
        var videoEvent = new VideoEvent
        {
            VideoPath = message.OriginalFilePath,
            Event = EventName,
            EventMessage = JsonSerializer.Serialize(message),
            EventDate = DateTime.Now
        };
        dbContext.Add(videoEvent);
        await dbContext.SaveChangesAsync();
    }

    private async ValueTask ConvertVideoPart(VideoPartResolutionSelected videoPartResolutionSelected)
    {
        string mediaPath = videoPartResolutionSelected.S3Key;
        string originalFilePath = videoPartResolutionSelected.OriginalFilePath;
        string resolution = videoPartResolutionSelected.Resolution.ToString();

        string rootMediaPath = _commonSettings.MediaPath;

        string videoPartPath = Path.Join(rootMediaPath, mediaPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaPath) ?? throw new InvalidOperationException("Could not get file name without extension.");

        string fullOriginalFilePath = Path.Join(rootMediaPath, originalFilePath);
        string originalFileDirectoryPath = Path.GetDirectoryName(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        Directory.CreateDirectory(Path.Combine(originalFileDirectoryPath, resolution));

        string convertedVideoPartPath = Path.Join(originalFileDirectoryPath, resolution, fileNameWithoutExtension + ".ts");
        string scale = $"scale=-2:{resolution}";
        await Cli.Wrap("ffmpeg")
                .WithArguments(args => args
                    .Add("-y")
                    .Add("-i").Add(videoPartPath)
                    .Add("-vf").Add(scale)
                    .Add("-crf").Add("30")
                    .Add("-c:v").Add("libx264")
                    .Add("-c:a").Add("aac")
                    .Add("-copyts")
                    .Add(convertedVideoPartPath)
                ).ExecuteAsync();
    }

    private async ValueTask UploadVideoPartToS3(VideoPartResolutionSelected videoPartResolutionSelected)
    {
        string mediaPath = videoPartResolutionSelected.S3Key;
        string originalFilePath = videoPartResolutionSelected.OriginalFilePath;
        string resolution = videoPartResolutionSelected.Resolution.ToString();

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

    private async ValueTask PublishVideoConvertedEvent(VideoPartResolutionSelected videoPartResolutionSelected, Guid processingTaskId)
    {
        string videoPartPath = videoPartResolutionSelected.S3Key;
        string originalFilePath = videoPartResolutionSelected.OriginalFilePath;
        int resolution = videoPartResolutionSelected.Resolution;

        VideoPartConverted message = new()
        {
            S3Key = videoPartPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath,
            ProcessingTaskId = processingTaskId
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
