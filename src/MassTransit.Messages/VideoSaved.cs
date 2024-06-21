namespace MassTransit.Messages;

public record VideoSaved
{
    public required string S3Key { get; set; }
}
