using Jobs.InitiateVideoProcessing.Settings;

namespace Jobs.InitiateVideoProcessing;

public static class RedisLockBuilder
{
    public static HostApplicationBuilder AddRedisLock(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
        builder.Services.AddSingleton<RedisLockHandler>();
        return builder;
    }
}
