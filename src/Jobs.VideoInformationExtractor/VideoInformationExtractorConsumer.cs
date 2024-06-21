using MassTransit;
using MassTransit.Messages;

namespace Jobs.VideoInformationExtractor;

public class VideoInformationExtractorConsumer(ILogger<VideoInformationExtractorConsumer> logger,
                                               IPublishEndpoint publishEndpoint) : IConsumer<VideoProccessed>
{
    public async Task Consume(ConsumeContext<VideoProccessed> context)
    {
        string mediaPath = context.Message.S3Key;

        logger.LogInformation(mediaPath);
        Console.WriteLine($"Video saved key: {mediaPath}");

        // Check if files exists in convention path

        // Download from S3 and save to convention path

        // Run ffmpeg to slice video into convention path

        // Upload each parts to S3 && publish messages to RabbitMQ

        // Publish successful message to RabbitMQ 
        
    }
}
