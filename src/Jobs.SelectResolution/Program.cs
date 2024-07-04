
using Jobs.SelectResolution;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CommonSettings>(builder.Configuration.GetSection("CommonSettings"));

builder.Services.AddDbContext<MediaProcessingDbContext>(options => 
                    options.UseSqlServer(builder.Configuration.GetConnectionString("MediaProcessMetadataConnectionString")));

builder.AddMassTransit();

var host = builder.Build();
host.Run();
