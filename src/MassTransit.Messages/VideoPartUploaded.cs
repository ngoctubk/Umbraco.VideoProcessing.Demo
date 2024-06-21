namespace MassTransit.Messages;

public class VideoPartUploaded
{
    public required string S3Key { get; set; }
}
