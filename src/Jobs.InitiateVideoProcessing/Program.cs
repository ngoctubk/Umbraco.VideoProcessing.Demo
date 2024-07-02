using Jobs.InitiateVideoProcessing;
using Jobs.InitiateVideoProcessing.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CommonSettings>(builder.Configuration.GetSection("CommonSettings"));

builder.Services.AddDbContext<MediaProcessingDbContext>(options => 
                    options.UseSqlServer(builder.Configuration.GetConnectionString("MediaProcessMetadataConnectionString")));

builder.AddAmazonS3();
builder.AddMassTransit();
builder.AddRedisLock();

var host = builder.Build();
await host.RunAsync();
