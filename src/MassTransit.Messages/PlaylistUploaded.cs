namespace MassTransit.Messages;

public class PlaylistUploaded
{
    public required string S3Key { get; set; }
    public required string OriginalFileS3Key { get; set; }
}
