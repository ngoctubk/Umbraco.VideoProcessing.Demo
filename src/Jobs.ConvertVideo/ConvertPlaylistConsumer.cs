using MassTransit;
using MassTransit.Messages;

namespace Jobs.ConvertVideo;

public class ConvertPlaylistConsumer(PlaylistConvertorService playlistConvertorService) : IConsumer<PlaylistResolutionSelected>
{
    public async Task Consume(ConsumeContext<PlaylistResolutionSelected> context)
    {
        await playlistConvertorService.Convert(context.Message);
    }
}
