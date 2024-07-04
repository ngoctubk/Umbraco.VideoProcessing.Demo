namespace MassTransit.Messages;

public class VideoPartConverted
{
    public required string S3Key { get; set; }
    public required int Resolution { get; set; }
    public required string OriginalFilePath { get; set; }
    public required Guid ProcessingTaskId { get; set; }
}
