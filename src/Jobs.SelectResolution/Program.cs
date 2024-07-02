
using Jobs.SelectResolution;

var builder = Host.CreateApplicationBuilder(args);

builder.AddMassTransit();

var host = builder.Build();
host.Run();
