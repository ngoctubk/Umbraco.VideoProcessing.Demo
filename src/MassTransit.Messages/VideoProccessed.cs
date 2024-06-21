namespace MassTransit.Messages;

public record VideoProccessed
{
    public required string S3Key { get; set; }
}
