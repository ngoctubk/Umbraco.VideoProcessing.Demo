using System.Text.Json;
using Commons;
using MassTransit;
using MassTransit.Messages;
using Microsoft.EntityFrameworkCore;

namespace Jobs.MonitorProcessing;

public class MonitorVideoPartConvertedConsumer(MediaProcessingDbContext dbContext) : IConsumer<VideoPartConverted>
{
    private const string EventName = "VideoPartConverted";
    public async Task Consume(ConsumeContext<VideoPartConverted> context)
    {
        // Catch VideoPartConverted event, so the VideoPartConverted worker finished its job. Mark ProcessingTask's IsDone true
        DateTime currentDate = DateTime.Now;
        
        VideoPartConverted videoPartConverted = context.Message;
        
        var processingTask = await dbContext.ProcessingTasks.SingleAsync(t => t.Id == videoPartConverted.ProcessingTaskId);
        processingTask.IsDone = true;
        processingTask.ModifiedDate = currentDate;

        var videoEvent = new VideoEvent
        {
            VideoPath = videoPartConverted.OriginalFilePath,
            Event = EventName,
            EventMessage = JsonSerializer.Serialize(videoPartConverted),
            EventDate = currentDate
        };
        dbContext.Add(videoEvent);
        await dbContext.SaveChangesAsync();
    }
}
