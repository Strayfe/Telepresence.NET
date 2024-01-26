using System.Text.RegularExpressions;
using k8s;

namespace Telepresence.NET.Connection;

public partial class Connection
{
    /// <summary>
    /// List of CIDR that will be allowed to conflict with local subnets.
    /// </summary>
    public IEnumerable<string>? AllowConflictingSubnets
    {
        get => _allowConflictingSubnets;
        init
        {
            if (value is null)
            {
                _allowConflictingSubnets = value;
                _arguments.Remove(nameof(AllowConflictingSubnets));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(AllowConflictingSubnets));

            _allowConflictingSubnets = value;

            var validSubnets = value.Where(x => !string.IsNullOrWhiteSpace(x));
            var subnets = string.Join(',', validSubnets);

            var arguments = new[]
            {
                "--allow-conflicting-subnets",
                subnets
            };

            _arguments[nameof(AllowConflictingSubnets)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _allowConflictingSubnets;
    
    /// <summary>
    /// Additional list of CIDR to proxy.
    /// </summary>
    public IEnumerable<string>? AlsoProxy
    {
        get => _alsoProxy;
        init
        {
            if (value is null)
            {
                _alsoProxy = value;
                _arguments.Remove(nameof(AlsoProxy));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(AlsoProxy));

            _alsoProxy = value;

            var networks = string.Join(',', value.Where(x => !string.IsNullOrWhiteSpace(x)));

            var arguments = new[]
            {
                "--also-proxy",
                networks
            };

            _arguments[nameof(AlsoProxy)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _alsoProxy;

    /// <summary>
    /// Start, or connect to, daemon in a docker container.
    /// </summary>
    public bool? Docker
    {
        get => _docker;
        init
        {
            if (value is null)
            {
                _docker = value;
                _arguments.Remove(nameof(Docker));
                return;
            }

            _docker = value;

            var arguments = new[]
            {
                "--docker"
            };

            _arguments[nameof(Docker)] = arguments;
        }
    }
    private readonly bool? _docker;
    
    /// <summary>
    /// Ports that a containerized daemon will expose. See docker run -p for more info.
    /// </summary>
    public IEnumerable<string>? Expose
    {
        get => _expose;
        init
        {
            if (value is null)
            {
                _expose = value;
                _arguments.Remove(nameof(Expose));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(Expose));

            _expose = value;

            var arguments = new List<string>();

            foreach (var port in value.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                arguments.Add("--expose");
                arguments.Add(port);
            }

            _arguments[nameof(Expose)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _expose;
    
    /// <summary>
    /// Hostname used by a containerized daemon.
    /// </summary>
    private string? Hostname
    {
        get => _hostname;
        init
        {
            if (value is null)
            {
                _hostname = value;
                _arguments.Remove(nameof(Hostname));
                return;
            }

            _hostname = value;

            var arguments = new[]
            {
                "--hostname",
                value
            };

            _arguments[nameof(Hostname)] = arguments;
        }
    }
    private readonly string? _hostname;
    
    /// <summary>
    /// The namespace where the traffic manager is to be found.
    /// </summary>
    public string? ManagerNamespace
    {
        get => _managerNamespace;
        init
        {
            if (value is null)
            {
                _managerNamespace = value;
                _arguments.Remove(nameof(ManagerNamespace));
                return;
            }
            
            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
        
            const string pattern = @"^[a-z0-9][a-z0-9-]*$|\{\{";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _managerNamespace = value;

            var arguments = new[]
            {
                "--manager-namespace",
                value
            };

            _arguments[nameof(ManagerNamespace)] = arguments;
        }
    }
    private readonly string? _managerNamespace;
    
    /// <summary>
    /// The namespaces that Telepresence will be concerned with.
    /// </summary>
    public IEnumerable<string>? MappedNamespaces
    {
        get => _mappedNamespaces;
        init
        {
            if (value is null)
            {
                _mappedNamespaces = value;
                _arguments.Remove(nameof(MappedNamespaces));
                return;
            }
            
            if (value.Any(@namespace => @namespace.Length > 64))
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);

            const string pattern = @"^[a-z0-9][a-z0-9-]$|\{\{";
        
            if (value.Any(@namespace => !Regex.IsMatch(@namespace, pattern)))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _mappedNamespaces = value;

            var namespaces = string.Join(',', value.Where(x => !string.IsNullOrWhiteSpace(x)));

            var arguments = new[]
            {
                "--mapped-namespaces",
                namespaces
            };

            _arguments[nameof(MappedNamespaces)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _mappedNamespaces;
    
    /// <summary>
    /// The name to use for this connection.
    /// Defaults to '<see cref="Context">&lt;context&gt;</see>-<see cref="Namespace">&lt;namespace&gt;</see>'.
    /// </summary>
    public string Name
    {
        get => _name ??= $"{Context}-{Namespace}";
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

            var arguments = new[]
            {
                "--name",
                value
            };

            _arguments[nameof(Name)] = arguments;
        }
    }
    private string? _name;
    
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

            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
        
            const string pattern = @"^[a-z0-9][a-z0-9-]*$|\{\{";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _namespace = value;

            var arguments = new[]
            {
                "--namespace",
                value
            };

            _arguments[nameof(Namespace)] = arguments;
        }
    }
    private string? _namespace;
    
    /// <summary>
    /// List of CIDR to never proxy.
    /// </summary>
    public IEnumerable<string>? NeverProxy
    {
        get => _neverProxy;
        init
        {
            if (value is null)
            {
                _neverProxy = value;
                _arguments.Remove(nameof(NeverProxy));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(NeverProxy));

            _neverProxy = value;

            var networks = string.Join(',', value.Where(x => !string.IsNullOrWhiteSpace(x)));

            var arguments = new[]
            {
                "--never-proxy",
                networks
            };

            _arguments[nameof(NeverProxy)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _neverProxy;
}