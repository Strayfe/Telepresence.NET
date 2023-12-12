using System.Reflection;

namespace Telepresence.NET;

internal static class Defaults
{
    /// <summary>
    /// A default name that can be used throughout convention-based creation of an intercept specification.
    /// </summary>
    internal static readonly string Name = Assembly
                                               .GetEntryAssembly()?
                                               .GetName()
                                               .Name?
                                               .Replace('.', '-')
                                               .Replace('_', '-')
                                               .ToLowerInvariant() ?? 
                                           throw new InvalidOperationException(Exceptions.CantDetermineName);
}