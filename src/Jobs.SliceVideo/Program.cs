
using Jobs.SliceVideo;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CommonSettings>(builder.Configuration.GetSection("CommonSettings"));

builder.AddAmazonS3();
builder.AddMassTransit();

var host = builder.Build();
host.Run();
