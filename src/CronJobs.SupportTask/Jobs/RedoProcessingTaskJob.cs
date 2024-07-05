using Quartz;

namespace CronJobs.SupportTask;


[DisallowConcurrentExecution]
public class RedoProcessingTaskJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException();
    }
}
