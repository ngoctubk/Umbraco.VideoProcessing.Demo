using Amazon.S3;
using Microsoft.Extensions.Options;

namespace Jobs.SliceVideo;

public static class AmazonS3Builder
{
    public static HostApplicationBuilder AddAmazonS3(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<S3StorageSettings>(builder.Configuration.GetSection("S3Storage"));
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<S3StorageSettings>>().Value);
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var clientConfig = new AmazonS3Config
            {
                AuthenticationRegion = builder.Configuration["S3Storage:Region"],
                ServiceURL = builder.Configuration["S3Storage:ServiceUrl"],
                ForcePathStyle = true
            };
            return new AmazonS3Client(
                builder.Configuration["S3Storage:AccessKey"],
                builder.Configuration["S3Storage:SecretKey"],
                clientConfig);
        });
        return builder;
    }
}
