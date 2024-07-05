using Commons;
using MassTransit;
using MassTransit.Messages;
using Microsoft.EntityFrameworkCore;

namespace Jobs.MonitorProcessing;

public class MonitorVideoSavedConsumer(MediaProcessingDbContext dbContext) : IConsumer<VideoCut>
{
    public async Task Consume(ConsumeContext<VideoCut> context)
    {
        // Catch VideoCut event, so the VideoCut worker finished its job. Mark video's IsCut true
        string mediaPath = context.Message.S3Key;
        Video video = await dbContext.Videos.SingleAsync(v => v.VideoPath == mediaPath);
        video.IsCut = true;
        video.ModifiedDate = DateTime.Now;
        await dbContext.SaveChangesAsync();
    }
}
