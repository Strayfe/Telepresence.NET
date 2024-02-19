namespace Telepresence.NET.Intercept;

/// <summary>
/// Opinionated custom flags created by me to ease the flow of debugging .NET applications.
/// </summary>
/// <remarks>
/// These are non-standard and do not conform to the current Telepresence CLI.
/// </remarks>
public partial class Intercept
{
    // todo: environment injector strategy

    /// <summary>
    /// Ask the library to automatically try injecting environment variables into the running process in the most secure
    /// way possible, i.e. not leaving a sensitive file on the filesystem.
    /// </summary>
    public bool? InjectEnvironment
    {
        get => _injectEnvironment;
        init
        {
            if (value is null)
            {
                _injectEnvironment = value;
                _arguments.Remove(nameof(InjectEnvironment));
                return;
            }

            _injectEnvironment = value;
        }
    }
    private readonly bool? _injectEnvironment;

    /// <summary>
    /// Provide a list of environment variables to be used to override variables set by the cluster or that need
    /// to be explicitly set for your debug instance to work correctly.
    /// </summary>
    public IEnumerable<KeyValuePair<string,string>>? IncludeEnvironment { get; init; }

    /// <summary>
    /// Provide a list of environment variables to be excluded from import into the debug instance.
    /// This can be useful when certain environment variables in the cluster pod will break your local debug but don't
    /// want to provide an override with <see cref="IncludeEnvironment"/>.
    /// </summary>
    public IEnumerable<string>? ExcludeEnvironment { get; init; }
}