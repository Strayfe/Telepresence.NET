using Microsoft.Extensions.DependencyInjection;
using Telepresence.NET.HeaderPropagation.Mvc.DelegatingHandlers;
using Telepresence.NET.HeaderPropagation.Mvc.Filters;
using Telepresence.NET.RestfulApi;

namespace Telepresence.NET.DependencyInjection;

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
    public static TelepresenceBuilder WithHttpRequestForwarding(this TelepresenceBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<TelepresenceActionFilter>();
        builder.Services.AddScoped<TelepresenceDelegatingHandler>();

        return builder;
    }

    /// <summary>
    /// Registers dependencies and services to interact with the Telepresence RESTful API.
    /// </summary>
    /// <remarks>
    /// This is required if you intend to use the MassTransit header propagation for events.
    /// </remarks>
    public static TelepresenceBuilder WithRestfulApi(this TelepresenceBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        // todo: consider registering a typed client for the TelepresenceApiService
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<ITelepresenceApiService, TelepresenceApiService>();

        return builder;
    }
}
