﻿using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using CliWrap;
using Commons;
using Jobs.InitiateVideoProcessing.Settings;
using MassTransit;
using MassTransit.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Jobs.InitiateVideoProcessing;

public class InitiateVideoProcessingConsumer(IPublishEndpoint publishEndpoint,
                                             IOptions<CommonSettings> optionCommonSettings,
                                             S3StorageSettings s3StorageSettings,
                                             IAmazonS3 amazonS3,
                                             MediaProcessingDbContext dbContext,
                                             RedisLockHandler redisLockHandler) : IConsumer<VideoSaved>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    private const string EventName = "InitiateVideoProcessing";
    private const string _masterPlaylistFile = "MasterPlaylist.m3u8";
    private const string _masterPlaylistReplaceText = "VideoFileS3StoragePath";

    public async Task Consume(ConsumeContext<VideoSaved> context)
    {
        string mediaPath = context.Message.S3Key;

        // Make sure that only 1 consumer is processing 1 video (with the same mediaPath) at a time.
        var expiry = TimeSpan.FromSeconds(30);
        var wait = TimeSpan.FromSeconds(5);
        var retry = TimeSpan.FromSeconds(1);
        await redisLockHandler
                    .PerformActionWithLock(mediaPath, expiry, wait, retry, async () =>
                    {
                        await InitiateVideoProcessing(mediaPath);
                    });
    }

    private async ValueTask InitiateVideoProcessing(string mediaPath)
    {
        await ExtractVideoInformation(mediaPath);
        await CreateMasterPlaylist(mediaPath);
        await PublishVideoProccessedEvent(mediaPath);
    }

    private async ValueTask ExtractVideoInformation(string mediaPath)
    {
        string rootMediaPath = _commonSettings.MediaPath;
        string fullVideoPath = Path.Join(rootMediaPath, mediaPath);
        string directoryPath = Path.GetDirectoryName(fullVideoPath) ?? throw new InvalidOperationException("Could not get directory.");
        var videoName = Path.GetFileName(fullVideoPath);

        Video? video = await dbContext.Videos.FirstOrDefaultAsync(v => v.VideoPath == mediaPath);
        DateTime eventDate = DateTime.Now;
        if (video is null)
        {
            // Video does not exist
            video = new Video
            {
                VideoPath = mediaPath,
                IsExtracted = false,
                IsCut = false,
                CreatedDate = eventDate
            };
            dbContext.Add(video);
        }

        if (!video.IsExtracted)
        {
            await DownloadVideoFromS3(mediaPath, fullVideoPath);
            string rawMetadata = await GetRawMetadataOfVideo(directoryPath, videoName);
            video.RawMetadata = rawMetadata;
            video.ModifiedDate = eventDate;
            video.IsExtracted = true;
        }

        var videoEvent = new VideoEvent
        {
            VideoPath = mediaPath,
            VideoId = video.Id,
            EventDate = eventDate,
            Event = EventName
        };
        dbContext.Add(videoEvent);

        await dbContext.SaveChangesAsync();
    }

    private async ValueTask CreateMasterPlaylist(string mediaPath)
    {
        string rootMediaPath = _commonSettings.MediaPath;
        string fullVideoPath = Path.Join(rootMediaPath, mediaPath);
        string directoryPath = Path.GetDirectoryName(fullVideoPath) ?? throw new InvalidOperationException("Could not get directory.");

        string masterPlaylistContent = File.ReadAllText(_masterPlaylistFile);
        masterPlaylistContent = masterPlaylistContent.Replace(_masterPlaylistReplaceText, mediaPath);
        await File.WriteAllTextAsync(Path.Combine(directoryPath, _masterPlaylistFile), masterPlaylistContent);
    }

    private static async ValueTask<string> GetRawMetadataOfVideo(string directoryPath, string videoName)
    {
        var stdOutBuffer = new StringBuilder();
        await Cli.Wrap("ffprobe")
                    .WithArguments(args => args
                        .Add("-v").Add("quiet")
                        .Add("-print_format").Add("json")
                        .Add("-show_format")
                        .Add("-show_streams")
                        .Add(videoName)
                    )
                    .WithWorkingDirectory(directoryPath)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .ExecuteAsync();
        return stdOutBuffer.ToString();
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
            using var fileStream = File.Create(fullVideoPath);
            await response.ResponseStream.CopyToAsync(fileStream);
        }
    }

    private async ValueTask PublishVideoProccessedEvent(string mediaPath)
    {
        VideoProccessed message = new()
        {
            S3Key = mediaPath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}