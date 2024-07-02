using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using Jobs.ConvertVideo.Settings;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.ConvertVideo;

public class ConvertPlaylistConsumer(IOptions<CommonSettings> optionCommonSettings,
                                     IPublishEndpoint publishEndpoint,
                                     S3StorageSettings s3StorageSettings,
                                     IAmazonS3 amazonS3) : IConsumer<PlaylistResolutionSelected>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;

    public async Task Consume(ConsumeContext<PlaylistResolutionSelected> context)
    {
        string playlistPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFilePath;
        string resolution = context.Message.Resolution.ToString();
        
        string rootMediaPath = _commonSettings.MediaPath;
        string fullPlaylistPath = Path.Join(rootMediaPath, playlistPath);
        string fullOriginalFilePath = Path.Join(rootMediaPath, originalFilePath);

        string originalFileDirectoryPath = Path.GetDirectoryName(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get directory.");
        string prefixPath = Path.GetDirectoryName(originalFileDirectoryPath) ?? throw new InvalidOperationException("Could not get prefix directory.");
        string playlistName = Path.GetFileName(fullPlaylistPath) ?? throw new InvalidOperationException("Could not get file name."); ;
        string originalFileExtension = Path.GetExtension(fullOriginalFilePath) ?? throw new InvalidOperationException("Could not get extension.");

        await DownloadFromS3(playlistPath, fullPlaylistPath);

        string m3u8Content = File.ReadAllText(fullPlaylistPath);
        Directory.CreateDirectory(Path.Combine(prefixPath, resolution));
        string m3u8NewContent = Regex.Replace(m3u8Content, @$"(.*)({originalFileExtension})", Path.Combine(prefixPath, resolution, "$1" + ".ts"));
        await File.WriteAllTextAsync(Path.Combine(prefixPath, resolution, playlistName), m3u8NewContent);

        await PublishPlaylistConvertedEvent(playlistPath, context.Message.Resolution, originalFilePath);
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

    private async ValueTask PublishPlaylistConvertedEvent(string playlistPath, int resolution, string originalFilePath)
    {
        PlaylistConverted message = new()
        {
            S3Key = playlistPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
