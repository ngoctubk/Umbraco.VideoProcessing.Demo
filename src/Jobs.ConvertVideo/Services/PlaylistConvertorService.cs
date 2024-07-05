using System.Text.Json;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using Commons;
using Jobs.ConvertVideo.Settings;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.ConvertVideo;

public class PlaylistConvertorService(IOptions<CommonSettings> optionCommonSettings,
                                     IPublishEndpoint publishEndpoint,
                                     S3StorageSettings s3StorageSettings,
                                     MediaProcessingDbContext dbContext,
                                     IAmazonS3 amazonS3)
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    private const string EventName = "PlaylistResolutionSelected";
    private const string TaskName = "PlaylistResolutionConvert";

    public async ValueTask Convert(PlaylistResolutionSelected playlistResolutionSelected)
    {
        await AddPlaylistResolutionSelectedEvent(playlistResolutionSelected);

        Guid processingTaskId = await AddPlaylistResolutionConvertTask(playlistResolutionSelected);

        string playlistPath = playlistResolutionSelected.S3Key;
        string originalFilePath = playlistResolutionSelected.OriginalFilePath;
        string resolution = playlistResolutionSelected.Resolution.ToString();

        string rootMediaPath = _commonSettings.MediaPath;
        string fullPlaylistPath = Path.Join(rootMediaPath, playlistPath);
        string fullOriginalFilePath = Path.Join(rootMediaPath, originalFilePath);

        string originalFileDirectoryPath = Path.GetDirectoryName(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        string playlistName = Path.GetFileName(fullPlaylistPath) ?? throw new InvalidOperationException("Could not get file name."); ;
        string originalFileExtension = Path.GetExtension(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get extension.");

        await DownloadFromS3(playlistPath, fullPlaylistPath);

        // Write m3u8 file 
        string m3u8Content = File.ReadAllText(fullPlaylistPath);
        Directory.CreateDirectory(Path.Combine(originalFileDirectoryPath, resolution));
        string originalFileRelativeDirectoryPath = originalFileDirectoryPath.Replace(rootMediaPath, "");
        string m3u8NewContent = Regex.Replace(m3u8Content, @$"(.*)({originalFileExtension})", Path.Combine(originalFileRelativeDirectoryPath, resolution, "$1" + ".ts"));
        string playlistAbsolutePath = Path.Combine(originalFileDirectoryPath, resolution, playlistName);
        await File.WriteAllTextAsync(playlistAbsolutePath, m3u8NewContent);

        // Upload to S3
        using FileStream uploadFileStream = new(playlistAbsolutePath, FileMode.Open, System.IO.FileAccess.Read);
        string playlistRelativePath = playlistAbsolutePath.Replace(rootMediaPath, "");
        var uploadRequest = new PutObjectRequest
        {
            BucketName = s3StorageSettings.BucketName,
            Key = playlistRelativePath,
            InputStream = uploadFileStream
        };
        var uploadResponse = await amazonS3.PutObjectAsync(uploadRequest);

        await PublishPlaylistConvertedEvent(playlistPath, playlistResolutionSelected.Resolution, originalFilePath, processingTaskId);
    }

    private async ValueTask<Guid> AddPlaylistResolutionConvertTask(PlaylistResolutionSelected message)
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

    private async ValueTask AddPlaylistResolutionSelectedEvent(PlaylistResolutionSelected message)
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

    private async ValueTask DownloadFromS3(string mediaPath, string fullVideoPath)
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

    private async ValueTask PublishPlaylistConvertedEvent(string playlistPath, int resolution, string originalFilePath, Guid processingTaskId)
    {
        PlaylistConverted message = new()
        {
            S3Key = playlistPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath,
            ProcessingTaskId = processingTaskId
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
