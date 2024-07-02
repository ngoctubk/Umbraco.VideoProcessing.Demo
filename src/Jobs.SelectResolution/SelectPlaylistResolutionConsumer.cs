using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;

namespace Jobs.SelectResolution;

public class SelectPlaylistResolutionConsumer(IOptions<CommonSettings> optionCommonSettings,
                                              IPublishEndpoint publishEndpoint) : IConsumer<PlaylistUploaded>
{
    private readonly CommonSettings _commonSettings = optionCommonSettings.Value;

    public async Task Consume(ConsumeContext<PlaylistUploaded> context)
    {
        string mediaPath = context.Message.S3Key;
        string originalFilePath = context.Message.OriginalFileS3Key;
        List<int> resolutions = _commonSettings.Resolutions;

        foreach (var resolution in resolutions)
        {
            await PublishPlaylistResolutionSelectedEvent(mediaPath, resolution, originalFilePath);
        }
    }

    private async ValueTask PublishPlaylistResolutionSelectedEvent(string mediaPath, int resolution, string originalFilePath)
    {
        PlaylistResolutionSelected message = new()
        {
            S3Key = mediaPath,
            Resolution = resolution,
            OriginalFilePath = originalFilePath
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
