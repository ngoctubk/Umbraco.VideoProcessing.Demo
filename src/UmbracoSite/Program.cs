using Common.Umbraco.StorageProviders.S3.DependencyInjection;
using MassTransit;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Notifications;
using UmbracoSite;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MediaExtensionsOption>(builder.Configuration.GetSection(MediaExtensionsOption.MediaExtensions));

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddS3MediaFileSystemWithImageSharpCache()
    .AddS3VideoProviders()
    .AddNotificationAsyncHandler<MediaSavedNotification, MediaNotificationHandler>()
    .Build();

builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.SetKebabCaseEndpointNameFormatter();
    
    busConfiguration.UsingRabbitMq((context, configurator) =>
    {
        MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();
        configurator.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });
    });
});

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
