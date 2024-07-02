namespace Jobs.InitiateVideoProcessing.Settings;

public class RedisSettings
{
    public required string[] RedisEndpoints { get; set; }
    public int ConnectTimeout { get; set; } = 5000;
}
