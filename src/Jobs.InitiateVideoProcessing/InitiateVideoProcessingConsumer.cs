using MassTransit;
using MassTransit.Messages;

namespace Jobs.InitiateVideoProcessing;

public class InitiateVideoProcessingConsumer(ILogger<InitiateVideoProcessingConsumer> logger,
                                             IPublishEndpoint publishEndpoint) : IConsumer<VideoSaved>
{
    public async Task Consume(ConsumeContext<VideoSaved> context)
    {
        string mediaPath = context.Message.S3Key;
        logger.LogInformation(mediaPath);
        Console.WriteLine($"Video saved key: {mediaPath}");

        await publishEndpoint.Publish<VideoProccessed>(new VideoProccessed
        {
            S3Key = mediaPath
        }, context =>
        {
            context.Durable = true;
        });


        // Check if video has been processed (exists video processed record)
        // Insert records -> if success -> not exists -> if exception -> check if record is exists -> ...
        // If not then 
        // -> send message to initiate video processing pipeline
        // Else
        // -> return success to skip video processing
    }
}