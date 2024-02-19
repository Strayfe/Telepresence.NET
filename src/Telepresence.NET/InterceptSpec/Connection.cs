using System.Text.RegularExpressions;
using k8s;

namespace Telepresence.NET.InterceptSpec;

/// <summary>
/// Connection properties to use when Telepresence connects to the cluster.
/// </summary>
internal sealed class Connection
{
    private string? _name;
    private string? _namespace;
    private readonly string? _managerNamespace;
    private readonly IEnumerable<string>? _mappedNamespaces;

    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection()
    {
    }

    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection(string name) => Name = name;

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
    public string Context { get; init; } = KubernetesClientConfiguration
        .BuildConfigFromConfigFile()
        .CurrentContext;

    /// <summary>
    /// The name of the kubeconfig user to use.
    /// </summary>
    public string? User { get; init; }

    /// <summary>
    /// The name to use for this connection.
    /// Defaults to '<see cref="Context">&lt;context&gt;</see>-<see cref="Namespace">&lt;namespace&gt;</see>'.
    /// </summary>
    public string Name
    {
        // strange issue with not accepting underscores in the name so normalizing it here
        get => _name ??= $"{Context}-{Namespace}"
            .Replace('_', '-')
            .ToLowerInvariant();
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

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
    public string Namespace
    {
        get => _namespace ??= KubernetesClientConfiguration.BuildConfigFromConfigFile().Namespace;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Namespace));

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
                throw new ArgumentNullException(nameof(ManagerNamespace));

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
                throw new ArgumentNullException(nameof(MappedNamespaces));

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