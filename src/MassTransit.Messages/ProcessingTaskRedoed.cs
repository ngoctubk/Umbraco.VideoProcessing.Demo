namespace MassTransit.Messages;

public class ProcessingTaskRedoed
{
    public Guid TaskId { get; set; }
    public required string MediaPartPath { get; set; }
    public required int Resolution { get; set; }
    public required string OriginalFilePath { get; set; }
    public required string TaskName { get; set; }
}
