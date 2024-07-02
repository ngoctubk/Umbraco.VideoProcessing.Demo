using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.SelectResolution;

public class SelectVideoPartResolutionConsumer(IOptions<CommonSettings> optionCommonSettings,
                                              IPublishEndpoint publishEndpoint) : IConsumer<VideoPartUploaded>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;
    
    public async Task Consume(ConsumeContext<VideoPartUploaded> context)
    {
        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFileS3Key;
        List<int> resolutions = _commonSettings.Resolutions;

        foreach (var resolution in resolutions)
        {
            await PublishVideoPartResolutionSelectedEvent(mediaPath, resolution, originalFilePath);
        }
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
