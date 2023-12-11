using System.Text.RegularExpressions;

namespace Telepresence.NET.Models.Intercept;

/// <summary>
/// Connection properties to use when Telepresence connects to the cluster.
/// </summary>
public sealed class Connection
{
    private readonly string? _name;
    private readonly string? _namespace;
    private readonly string? _managerNamespace;
    private readonly IEnumerable<string>? _mappedNamespaces;

    public Connection()
    {
    }

    public Connection(string context) => Context = context;

    public Connection(string context, string @namespace)
    {
        Context = context;
        Namespace = @namespace;
        Name = _name = $"{Context}-{Namespace}".ToLowerInvariant();;
    }

    /// <summary>
    /// Username to impersonate for the operation.
    /// </summary>
    public string? As { get; init; }
    
    /// <summary>
    /// Groups to impersonate for the operation.
    /// </summary>
    public IEnumerable<string>? AsGroups { get; init; }
    
    /// <summary>
    /// UID to impersonate for the operation.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? AsUID { get; init; }
    
    /// <summary>
    /// The name of the kubeconfig cluster to use.
    /// </summary>
    public string? Cluster { get; init; }
    
    /// <summary>
    /// The name of the kubeconfig context to use. Defaults to the current context of the kubeconfig.
    /// </summary>
    public string? Context { get; init; }
    
    /// <summary>
    /// The name of the kubeconfig user to use.
    /// </summary>
    public string? User { get; init; }

    /// <summary>
    /// The name to use for this connection.
    /// Defaults to '<see cref="Context">&lt;context&gt;</see>-<see cref="Namespace">&lt;namespace&gt;</see>'.
    /// </summary>
    public string? Name
    {
        get => _name;
        init
        {
            if (string.IsNullOrWhiteSpace(_name) &&
                !string.IsNullOrWhiteSpace(Context) &&
                !string.IsNullOrWhiteSpace(Namespace))
            {
                _name = $"{Context}-{Namespace}".ToLowerInvariant();

                return;
            }

            // todo: determine and set the context and default namespace from the current kubeconfig 
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
            
            const string pattern = "^[a-z][a-z0-9-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _name = value;
        }
    }

    /// <summary>
    /// The namespace that this connection is bound to.
    /// Defaults to the default appointed by the context.
    /// </summary>
    public string? Namespace
    {
        get => _namespace;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            
            const string pattern = "^[a-z0-9][a-z0-9-]{1,62}$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _namespace = value;
        }
    }

    /// <summary>
    /// The namespace where the traffic manager is to be found.
    /// </summary>
    public string? ManagerNamespace
    {
        get => _managerNamespace;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            
            const string pattern = "^[a-z0-9][a-z0-9-]{1,62}$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _managerNamespace = value;
        }
    }

    /// <summary>
    /// The namespaces that Telepresence will be concerned with.
    /// </summary>
    public IEnumerable<string>? MappedNamespaces
    {
        get => _mappedNamespaces;
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            const string pattern = "^[a-z0-9][a-z0-9-]{1,62}$";
            
            if (value.Any(@namespace => !Regex.IsMatch(@namespace, pattern)))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _mappedNamespaces = value;
        }
    }

    /// <summary>
    /// Additional list of CIDR to proxy.
    /// </summary>
    public IEnumerable<string>? AlsoProxy { get; init; }
    
    /// <summary>
    /// List of CIDR to never proxy.
    /// </summary>
    public IEnumerable<string>? NeverProxy { get; init; }
}