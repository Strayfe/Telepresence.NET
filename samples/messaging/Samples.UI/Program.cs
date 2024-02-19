using System.Reflection;
using MassTransit;
using MassTransit.Metadata;
using Samples.UI;
using Telepresence.NET.HeaderPropagation.MassTransit.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // if running locally, create queues for the current user, in the consumer definition this can be made temporary
    // and the registered filters should force messages not intended for a specific instance to skip and we can discard
    // those too to avoid many/large skipped queues
    if (!HostMetadataCache.IsRunningInContainer)
        x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter($"{Environment.UserName}:"));
    else
        x.SetKebabCaseEndpointNameFormatter();

    var entryAssembly = Assembly.GetEntryAssembly();

    x.AddConsumers(entryAssembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(HostMetadataCache.IsRunningInContainer ? "rabbitmq" : "localhost", "/");

        cfg.UseConsumeFilter(typeof(TelepresenceConsumeFilter<>), context);
        cfg.UseSendFilter(typeof(TelepresenceSendFilter<>), context);
        cfg.UsePublishFilter(typeof(TelepresencePublishFilter<>), context);

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        options.WaitUntilStarted = true;
        options.StartTimeout = TimeSpan.FromSeconds(30);
        options.StopTimeout = TimeSpan.FromSeconds(60);
    });

builder.Services.AddOptions<HostOptions>()
    .Configure(options =>
    {
        options.StartupTimeout = TimeSpan.FromSeconds(60);
        options.ShutdownTimeout = TimeSpan.FromSeconds(60);
    });

builder.Services.AddHostedService<StartupService>();

var app = builder.Build();

await app.RunAsync();
