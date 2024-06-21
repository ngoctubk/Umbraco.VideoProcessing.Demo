using Common.Umbraco.StorageProviders.S3.IO;

namespace Umbraco.VideoProviders.S3;

public class S3VideoProvider(IS3FileSystemProvider s3FileSystemProvider) : IVideoProvider
{
    public Stream GetVideo(string videoPath)
    {
        var s3FileSystem = s3FileSystemProvider.GetFileSystem(S3FileSystemOptions.MediaFileSystemName);
        return s3FileSystem.OpenFile(videoPath);
    }
}
