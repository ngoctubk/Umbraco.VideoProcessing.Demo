
using Jobs.SelectResolution;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CommonSettings>(builder.Configuration.GetSection("CommonSettings"));

builder.AddMassTransit();

var host = builder.Build();
host.Run();
