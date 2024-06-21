using Common.Umbraco.StorageProviders.S3.IO;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace UmbracoSite;

public class VideoS3ProvidersMiddleware(RequestDelegate next, IS3FileSystemProvider s3FileSystemProvider, IOptions<MediaExtensionsOption> options)
{
    private readonly MediaExtensionsOption _mediaExtensionsOption = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        string url = context.Request.GetDisplayUrl();
        bool isInvalidVideoUrl = !IsValidVideoRequest(context);
        if (isInvalidVideoUrl)
        {
            await next(context);
            return;
        }

        // -> get video key and resolution and playlist from query
        (bool isPlaylist, string playlistPath) = GetFileKey(context);
        if (!isPlaylist)
        {
            await next(context);
            return;
        }

        using Stream videoStream = GetVideo(playlistPath);

        await ReturnVideoToUser(context, videoStream);
    }

    private (bool isPlaylist, string playlistPath) GetFileKey(HttpContext context)
    {
        string? path = context.Request.Path.Value;
        IQueryCollection query = context.Request.Query;
        string? res = query.Where(x => x.Key == "res")
                        .Select(x => x.Value[^1])
                        .FirstOrDefault();
        if (string.IsNullOrEmpty(res))
        {
            return (false, string.Empty);
        }

        var directoryPath = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Could not get directory.");
         var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path) ?? throw new InvalidOperationException("Could not get file name without extension.");
        if (res.Equals("auto"))
        {
            return (true, Path.Combine(directoryPath, "MasterPlaylist.m3u8"));
        }
        else
        {
            return (true, Path.Combine(directoryPath, res, fileNameWithoutExtension + ".m3u8"));
        }
    }

    private bool IsValidVideoRequest(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();
        List<string> extensions = _mediaExtensionsOption.Videos;
        ReadOnlySpan<char> span;
        int num = url.IndexOf('?');
        if (num > -1)
        {
            span = url.AsSpan(0, num);
        }
        else
        {
            span = url;
        }
        if ((num = span.LastIndexOf('.')) != -1)
        {
            ReadOnlySpan<char> span2 = span.Slice(num + 1);
            foreach (string extension in extensions)
            {
                if (MemoryExtensions.Equals(span2, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Stream GetVideo(string videoPath)
    {
        var s3FileSystem = s3FileSystemProvider.GetFileSystem(S3FileSystemOptions.MediaFileSystemName);
        return s3FileSystem.OpenFile(videoPath);
    }

    private async Task ReturnVideoToUser(HttpContext context, Stream videoStream)
    {
        if (videoStream.CanSeek)
        {
            videoStream.Position = 0;
        }

        await videoStream.CopyToAsync(context.Response.Body);
    }
}

public static class VideoProvidersMiddlewareExtensions
{
    public static IApplicationBuilder UseS3VideoProvider(this IApplicationBuilder app)
    {
        app.UseMiddleware<VideoS3ProvidersMiddleware>();
        return app;
    }

    public static IUmbracoBuilder AddS3VideoProviders(this IUmbracoBuilder builder)
    {
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter("S3VideoProviders")
            {
                PrePipeline = prePipeline => prePipeline.UseS3VideoProvider()
            });
        });
        return builder;
    }
}