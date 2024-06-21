namespace Umbraco.VideoProviders.S3;

public interface IVideoProvider
{
    Stream GetVideo(string videoPath);
}
