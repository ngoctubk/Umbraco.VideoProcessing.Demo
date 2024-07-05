using Microsoft.Extensions.Options;
using Quartz;

namespace CronJobs.SupportTask;

[DisallowConcurrentExecution]
public class RemoveTempConvertedFileJob(IOptions<CommonSettings> optionCommonSettings) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        CommonSettings commonSettings = optionCommonSettings.Value;
        System.Console.WriteLine(commonSettings.MediaPath);
        
        await Task.Delay(1);
    }
}
