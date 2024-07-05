
using Jobs.ConvertVideo;
using Jobs.ConvertVideo.Settings;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CommonSettings>(builder.Configuration.GetSection("CommonSettings"));

builder.Services.AddDbContext<MediaProcessingDbContext>(options => 
                    options.UseSqlServer(builder.Configuration.GetConnectionString("MediaProcessMetadataConnectionString")));

builder.Services.AddTransient<PlaylistConvertorService>();
builder.Services.AddTransient<VideoPartConvertorService>();

builder.AddAmazonS3();
builder.AddMassTransit();

var host = builder.Build();
host.Run();
