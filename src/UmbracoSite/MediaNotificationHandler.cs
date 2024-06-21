using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbracoSite;

public class MediaNotificationHandler(IOptions<MediaExtensionsOption> options,
                                      IPublishEndpoint publishEndpoint) : INotificationAsyncHandler<MediaSavedNotification>
{
    private readonly MediaExtensionsOption _options = options.Value;

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var mediaItem in notification.SavedEntities)
        {
            var fileExtension = mediaItem.GetValue<string>("umbracoExtension");
            if (!string.IsNullOrEmpty(fileExtension) && _options.Videos.Contains(fileExtension))
            {
                var mediaPath = mediaItem.GetValue<string>("umbracoFile") ?? string.Empty;
                await publishEndpoint.Publish<VideoSaved>(new VideoSaved
                {
                    S3Key = mediaPath
                }, context =>
                {
                    context.Durable = true;
                }, cancellationToken: cancellationToken);
            }
        }
    }
}