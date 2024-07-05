using Quartz;

namespace CronJobs.SupportTask;

public static class QuartzJobsBuilder
{
    public static HostApplicationBuilder AddQuartzJobs(this HostApplicationBuilder builder)
    {
        builder.Services.AddQuartz(q =>
        {
            // q.AddJobAndTrigger<RedoProcessingTaskJob>(builder.Configuration);
            q.AddJobAndTrigger<RemoveTempConvertedFileJob>(builder.Configuration);
            // q.AddJobAndTrigger<RequeueVideoProcessingJob>(builder.Configuration);
        });

        builder.Services.AddQuartzHostedService(options =>
                {
                    // when shutting down we want jobs to complete gracefully
                    options.WaitForJobsToComplete = true;
                });

        return builder;
    }
}
