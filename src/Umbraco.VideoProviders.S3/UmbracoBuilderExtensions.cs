using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.VideoProviders.S3;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddVideoProviders(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IVideoProvider, S3VideoProvider>();

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter("VideoProviders")
            {
                PrePipeline = prePipeline => prePipeline.UseVideoProvider()
            });
        });
        return builder;
    }
}
