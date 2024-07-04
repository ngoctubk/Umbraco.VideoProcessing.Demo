using System.Text.Json;
using Commons;
using MassTransit;
using MassTransit.Messages;
using Microsoft.EntityFrameworkCore;

namespace Jobs.MonitorProcessing;

public class MonitorPlaylistConvertedConsumer(MediaProcessingDbContext dbContext) : IConsumer<PlaylistConverted>
{
    private const string EventName = "PlaylistConverted";
    public async Task Consume(ConsumeContext<PlaylistConverted> context)
    {
        // Catch PlaylistConverted event, so the PlaylistConverted worker finished its job. Mark ProcessingTask's IsDone true
        DateTime currentDate = DateTime.Now;

        PlaylistConverted playlistConverted = context.Message;
        
        var processingTask = await dbContext.ProcessingTasks.SingleAsync(t => t.Id == playlistConverted.ProcessingTaskId);
        processingTask.IsDone = true;
        processingTask.ModifiedDate = currentDate;

        var videoEvent = new VideoEvent
        {
            VideoPath = playlistConverted.OriginalFilePath,
            Event = EventName,
            EventMessage = JsonSerializer.Serialize(playlistConverted),
            EventDate = DateTime.Now
        };
        dbContext.Add(videoEvent);
        await dbContext.SaveChangesAsync();
    }
}
