using MassTransit;
using MassTransit.Messages;

namespace Jobs.ConvertVideo;

public class ConvertVideoPartConsumer(VideoPartConvertorService videoPartConvertorService) : IConsumer<VideoPartResolutionSelected>
{
    public async Task Consume(ConsumeContext<VideoPartResolutionSelected> context)
    {
        await videoPartConvertorService.Convert(context.Message);
    }
}
