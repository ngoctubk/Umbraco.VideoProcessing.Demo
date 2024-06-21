
using Jobs.InitiateVideoProcessing;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.SetKebabCaseEndpointNameFormatter();

    busConfiguration.AddConsumer<InitiateVideoProcessingConsumer>();

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
