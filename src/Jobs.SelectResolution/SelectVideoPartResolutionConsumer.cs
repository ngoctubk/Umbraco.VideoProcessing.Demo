using System.Text.Json;
using Commons;
using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.SelectResolution;

public class SelectVideoPartResolutionConsumer(IOptions<CommonSettings> optionCommonSettings,
                                               MediaProcessingDbContext dbContext,
                                               IPublishEndpoint publishEndpoint) : IConsumer<VideoPartUploaded>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    private const string EventName = "VideoPartUploaded";

    public async Task Consume(ConsumeContext<VideoPartUploaded> context)
    {
        await AddPlaylistUploadedEvent(context.Message);

        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFileS3Key;
        
        List<int> resolutions = _commonSettings.Resolutions;

        foreach (var resolution in resolutions)
        {
            await PublishVideoPartResolutionSelectedEvent(mediaPath, resolution, originalFilePath);
        }
    }

    private async ValueTask AddPlaylistUploadedEvent(VideoPartUploaded message)
    {
        var videoEvent = new VideoEvent
        {
            VideoPath = message.OriginalFileS3Key,
            Event = EventName,
            EventMessage = JsonSerializer.Serialize(message),
            EventDate = DateTime.Now
        };
        dbContext.Add(videoEvent);
        await dbContext.SaveChangesAsync();
    }

    private async ValueTask PublishVideoPartResolutionSelectedEvent(string mediaPath, int resolution, string originalFilePath)
    {
        VideoPartResolutionSelected message = new()
        {
            S3Key = mediaPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
