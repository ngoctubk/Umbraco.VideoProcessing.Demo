using Quartz;

namespace CronJobs.SupportTask;

[DisallowConcurrentExecution]
public class RequeueVideoProcessingJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException();
    }
}
