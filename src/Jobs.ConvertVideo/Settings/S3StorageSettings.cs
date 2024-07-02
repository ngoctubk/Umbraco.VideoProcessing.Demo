namespace Jobs.ConvertVideo.Settings;

public class S3StorageSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string ServiceURL { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}
