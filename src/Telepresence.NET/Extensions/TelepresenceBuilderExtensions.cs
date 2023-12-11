using Microsoft.Extensions.DependencyInjection;
using Telepresence.NET.DelegatingHandlers;
using Telepresence.NET.Options;

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
        Action<DelegatingHandlerOptions>? configureOptions = null
        )
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        if (configureOptions is not null)
            builder.Services.Configure(configureOptions);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<TelepresenceDelegatingHandler>();

        return builder;
    }
}