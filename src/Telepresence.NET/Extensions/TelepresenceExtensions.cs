using Microsoft.Extensions.DependencyInjection;

namespace Telepresence.NET.Extensions;

/// <summary>
/// Exposes extensions to register the telepresence services.
/// </summary>
public static class TelepresenceExtensions
{
    /// <summary>
    /// Provides a common entry point for registering telepresence and its required dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TelepresenceBuilder AddTelepresence(this IServiceCollection services)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        services.AddOptions();

        return new TelepresenceBuilder(services);
    }

    /// <summary>
    /// Provides a common entry point for registering telepresence and its required dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddTelepresence(
        this IServiceCollection services,
        Action<TelepresenceBuilder> configuration)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        configuration(services.AddTelepresence());

        return services;
    }
}