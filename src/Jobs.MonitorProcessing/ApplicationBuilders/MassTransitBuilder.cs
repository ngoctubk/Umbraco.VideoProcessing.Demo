using MassTransit;
using Microsoft.Extensions.Options;

namespace Jobs.MonitorProcessing;

public static class MassTransitBuilder
{
    public static HostApplicationBuilder AddMassTransit(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

        builder.Services.AddMassTransit(busConfiguration =>
        {
            busConfiguration.SetKebabCaseEndpointNameFormatter();

            busConfiguration.AddConsumer<MonitorVideoSavedConsumer>();
            busConfiguration.AddConsumer<MonitorPlaylistConvertedConsumer>();
            busConfiguration.AddConsumer<MonitorVideoPartConvertedConsumer>();

            busConfiguration.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                configurator.UseKillSwitch(options => options
                                            .SetActivationThreshold(10)
                                            .SetTripThreshold(0.2)
                                            .SetRestartTimeout(m: 1));
                configurator.PrefetchCount = 50;
                MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();

                configurator.Host(new Uri(settings.Host), h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
                configurator.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
