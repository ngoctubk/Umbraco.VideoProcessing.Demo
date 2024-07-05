using Jobs.InitiateVideoProcessing.Settings;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Jobs.InitiateVideoProcessing;

public class RedisLockHandler : IDisposable
{
    private bool _disposedValue = false;
    private readonly RedLockFactory _redLockFactory;
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<RedisLockHandler> _logger;

    public RedisLockHandler(IOptions<RedisSettings> RedisSettings, ILogger<RedisLockHandler> logger)
    {
        _redisSettings = RedisSettings.Value;
        _logger = logger;
        _redLockFactory = CreateRedLockFactory();
    }

    private RedLockFactory CreateRedLockFactory()
    {
        var configurtaion = _redisSettings;

        var connectionMultiplexers = new List<RedLockMultiplexer>();
        foreach (var endpoint in configurtaion.RedisEndpoints)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { endpoint },
                ConnectTimeout = configurtaion.ConnectTimeout,
                AbortOnConnectFail = false,
                AllowAdmin = true,
                SyncTimeout = 5000,
            });
            connectionMultiplexers.Add(connectionMultiplexer);
        }

        return RedLockFactory.Create(connectionMultiplexers);
    }

    public async Task<bool> PerformActionWithLock(string resource, TimeSpan expirationTime, TimeSpan waitTime, TimeSpan retryCount, Func<Task> action)
    {
        await using var redLock = await _redLockFactory.CreateLockAsync(resource, expirationTime, waitTime, retryCount);
        if (!redLock.IsAcquired)
        {
            _logger.LogError("Could not acquire lock for resource {Resource}", resource);
            return false;
        }

        _logger.LogInformation("Lock acquired for resource {Resource}", resource);
        await action();

        return true;
    }

    public async Task<bool> PerformActionWithLock(string resource, TimeSpan expirationTime, Func<Task> action)
    {
        await using var redLock = await _redLockFactory.CreateLockAsync(resource, expirationTime);
        if (!redLock.IsAcquired)
        {
            _logger.LogError("Could not acquire lock for resource {Resource}", resource);
            return false;
        }

        _logger.LogWarning("Lock acquired for resource {Resource}", resource);
        await action();

        return true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _logger.LogDebug("Disposing RedisLockHandler");
                _redLockFactory.Dispose();
            }
            _disposedValue = true;
        }
    }
}
