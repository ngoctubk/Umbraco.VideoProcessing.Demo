
using Amazon.S3;
using Jobs._1080pConvertor;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));
builder.Services.Configure<S3StorageSettings>(builder.Configuration.GetSection("S3Storage"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);
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

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.SetKebabCaseEndpointNameFormatter();

    busConfiguration.AddConsumer<Res1080pConvertorConsumer>();

    busConfiguration.UsingRabbitMq((context, configurator) =>
    {
        MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();
        configurator.PrefetchCount = 10;
        configurator.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });
        configurator.ConfigureEndpoints(context);
    });
});


var host = builder.Build();
host.Run();
