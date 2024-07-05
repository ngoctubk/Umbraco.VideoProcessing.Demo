using MassTransit;
using MassTransit.Messages;

namespace Jobs.ConvertVideo;

public class RedoProcessingTaskConsumer : IConsumer<ProcessingTaskRedoRequested>
{
    private readonly PlaylistConvertorService playlistConvertorService;
    private readonly VideoPartConvertorService videoPartConvertorService;
    private readonly IPublishEndpoint publishEndpoint;
    private Dictionary<string, Func<ProcessingTaskRedoRequested, ValueTask>> _taskExecutors;

    public RedoProcessingTaskConsumer(PlaylistConvertorService playlistConvertorService,
                                        VideoPartConvertorService videoPartConvertorService,
                                        IPublishEndpoint publishEndpoint,
                                        MediaProcessingDbContext dbContext)
    {
        this.playlistConvertorService = playlistConvertorService;
        this.videoPartConvertorService = videoPartConvertorService;
        this.publishEndpoint = publishEndpoint;
        this._taskExecutors = new();
        _taskExecutors.Add("PlaylistResolutionConvert", ConvertPlaylist);
        _taskExecutors.Add("VideoPartResolutionConvert", ConvertVideoPart);
    }

    private async ValueTask ConvertPlaylist(ProcessingTaskRedoRequested processingTaskRedoRequested)
    {
        await playlistConvertorService.Convert(new PlaylistResolutionSelected
        {
            TaskId = processingTaskRedoRequested.TaskId,
            Resolution = processingTaskRedoRequested.Resolution,
            OriginalFilePath = processingTaskRedoRequested.OriginalFilePath,
            S3Key = processingTaskRedoRequested.MediaPartPath
        });
    }

    private async ValueTask ConvertVideoPart(ProcessingTaskRedoRequested processingTaskRedoRequested)
    {
        await videoPartConvertorService.Convert(new VideoPartResolutionSelected
        {
            TaskId = processingTaskRedoRequested.TaskId,
            OriginalFilePath = processingTaskRedoRequested.OriginalFilePath,
            Resolution = processingTaskRedoRequested.Resolution,
            S3Key = processingTaskRedoRequested.MediaPartPath
        });
    }

    public async Task Consume(ConsumeContext<ProcessingTaskRedoRequested> context)
    {
        ProcessingTaskRedoRequested processingTaskRedoRequested = context.Message;

        await _taskExecutors[processingTaskRedoRequested.TaskName](processingTaskRedoRequested);

        await PublishTaskRedoedEvent(processingTaskRedoRequested);
    }

    private async ValueTask PublishTaskRedoedEvent(ProcessingTaskRedoRequested processingTaskRedoRequested)
    {
        ProcessingTaskRedoed message = new()
        {
            TaskId = processingTaskRedoRequested.TaskId,
            Resolution = processingTaskRedoRequested.Resolution,
            OriginalFilePath = processingTaskRedoRequested.OriginalFilePath,
            MediaPartPath = processingTaskRedoRequested.MediaPartPath,
            TaskName = processingTaskRedoRequested.TaskName
        };
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        await publishEndpoint.Publish(message, context => context.Durable = true, cancellationTokenSource.Token);
    }
}
