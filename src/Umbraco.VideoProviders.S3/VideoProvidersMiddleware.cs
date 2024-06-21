using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Umbraco.VideoProviders.S3;

public class VideoProvidersMiddleware(RequestDelegate next, IVideoProvider videoProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsInvalidVideoRequest(context))
        {
            await next(context);
            return;
        }

        // -> get video key and resolution and playlist from query
        string videoPath = GetVideoPath();

        using Stream videoStream = videoProvider.GetVideo(videoPath);

        await ReturnVideoToUser(context, videoStream);
    }


    private bool IsInvalidVideoRequest(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();
        if (context.Request.Path.Value.Contains("mp4"))
            return false;
        return true;
    }


    private string GetVideoPath()
    {
        string videoPath = "/media/05ui0xrb/docs_product_icon_512dp-2x.png";
        return videoPath;
    }


    private static async Task ReturnVideoToUser(HttpContext context, Stream videoStream)
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
    public static IApplicationBuilder UseVideoProvider(this IApplicationBuilder app)
    {
        app.UseMiddleware<VideoProvidersMiddleware>();
        return app;
    }
}