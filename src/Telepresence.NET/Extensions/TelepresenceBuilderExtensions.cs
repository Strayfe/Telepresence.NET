using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telepresence.NET.DelegatingHandlers;
using Telepresence.NET.Options;
using Telepresence.NET.Services;

namespace Telepresence.NET.Extensions;

public static class TelepresenceBuilderExtensions
{
    /// <summary>
    /// Injects the required services to handle sending intercepted requests to the correct instance of downstream
    /// applications in the event they are also being intercepted.
    /// </summary>
    /// <remarks>
    /// Remember to inject the DelegatingHandler into your typed/named HttpClients.
    /// This can be done globally using the HttpClientBuilder during Startup.
    /// </remarks>
    /// <example>
    /// <code>
    /// services
    ///     .AddHttpClient&lt;YourTypedClient&gt;()
    ///     .AddHttpMessageHandler&lt;TelepresenceDelegatingHandler&gt;();
    ///
    /// 
    /// services
    ///     .AddHttpClient("YourNamedClient")
    ///     .AddHttpMessageHandler&lt;TelepresenceDelegatingHandler&gt;();
    /// </code>
    /// </example>
    public static TelepresenceBuilder WithRequestForwarding(
        this TelepresenceBuilder builder,
        Action<DelegatingHandlerOptions>? configureOptions = null)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        configureOptions ??= options =>
        {
            options.InterceptHeaderNames = new List<string>
            {
                Constants.Defaults.Headers.TelepresenceInterceptAs
                // todo: maybe consider trying to store the headers in env and grab them here
            };
        };

        builder.Services.Configure(configureOptions);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<TelepresenceDelegatingHandler>();

        return builder;
    }

    public static TelepresenceBuilder WithRestfulApi(this TelepresenceBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddHttpClient();
        builder.Services.TryAddTransient<ITelepresenceApiService, TelepresenceApiService>();

        return builder;
    }

    // /// <summary>
    // /// Attempts to automatically handle whether running instances of an application should consume messages from queues.
    // /// </summary>
    // /// <remarks>
    // /// Make sure you have enabled the Telepresence REST api in the cluster to enable this feature.
    // /// </remarks>
    // public static TelepresenceBuilder WithMassTransitConsumerHandler(
    //     this TelepresenceBuilder builder,
    //     Action<ConsumerHandlerOptions>? configureOptions = null)
    // {
    //     if (builder is null)
    //         throw new ArgumentNullException(nameof(builder));
    //     
    //     if (configureOptions is not null)
    //         builder.Services.Configure(configureOptions);
    //     
    //     builder.Services.AddHttpContextAccessor();
    //     
    //     return builder;
    // }
}