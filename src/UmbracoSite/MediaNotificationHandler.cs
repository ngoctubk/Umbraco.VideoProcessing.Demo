using MassTransit;
using MassTransit.Messages;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbracoSite;

public class MediaNotificationHandler(IOptions<MediaExtensionsOption> options,
                                      OutboxDbContext dbContext,
                                      IPublishEndpoint publishEndpoint) : INotificationAsyncHandler<MediaSavedNotification>
{
    private readonly MediaExtensionsOption _options = options.Value;

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
    {
        foreach (var mediaItem in notification.SavedEntities)
        {
            var fileExtension = mediaItem.GetValue<string>("umbracoExtension");
            if (IsVideoExtension(fileExtension))
            {
                var mediaPath = mediaItem.GetValue<string>("umbracoFile") ?? throw new ArgumentNullException("umbracoFile of saved media is null");

                VideoSaved message = new()
                {
                    S3Key = mediaPath
                };
                await publishEndpoint.Publish(message, context => context.Durable = true, cancellationToken: cancellationToken);
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        bool IsVideoExtension(string? fileExtension)
        {
            return !string.IsNullOrEmpty(fileExtension) && _options.Videos.Contains(fileExtension);
        }
    }
}