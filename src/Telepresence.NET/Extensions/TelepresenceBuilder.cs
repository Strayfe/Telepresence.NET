using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Telepresence.NET.Extensions;

/// <summary>
/// Provides a common entry point to configure telepresence.
/// </summary>
public class TelepresenceBuilder
{
    /// <summary>
    /// Initializes a new instance of <see cref="TelepresenceBuilder"/>
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TelepresenceBuilder(IServiceCollection services) => 
        Services = services ?? throw new ArgumentNullException(nameof(services));
    
    /// <summary>
    /// Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }
}