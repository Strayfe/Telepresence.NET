using k8s;

namespace Telepresence.NET.Connection;

public partial class Connection
{
    /// <summary>
    /// Username to impersonate for the operation.
    /// </summary>
    public string? As
    {
        get => _as;
        init
        {
            if (value is null)
            {
                _as = value;
                _arguments.Remove(nameof(As));
                return;
            }

            _as = value;

            var arguments = new[]
            {
                "--as",
                value
            };

            _arguments[nameof(As)] = arguments;
        }
    }
    private readonly string? _as;

    /// <summary>
    /// Groups to impersonate for the operation.
    /// </summary>
    public IEnumerable<string>? AsGroups
    {
        get => _asGroups;
        init
        {
            if (value is null)
            {
                _asGroups = value;
                _arguments.Remove(nameof(AsGroups));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(AsGroups));

            _asGroups = value;

            var arguments = new List<string>();

            foreach (var group in value)
            {
                arguments.Add("--as-group");
                arguments.Add(group);
            }

            _arguments[nameof(AsGroups)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _asGroups;
    
    /// <summary>
    /// UID to impersonate for the operation.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? AsUID
    {
        get => _asUid;
        init
        {
            if (value is null)
            {
                _asUid = value;
                _arguments.Remove(nameof(AsUID));
                return;
            }

            _asUid = value;

            var arguments = new[]
            {
                "--as-uid",
                value
            };

            _arguments[nameof(AsUID)] = arguments;
        }
    }
    private readonly string? _asUid;
    
    /// <summary>
    /// Default cache directory
    /// </summary>
    public string? CacheDirectory
    {
        get => _cacheDirectory;
        init
        {
            if (value is null)
            {
                _cacheDirectory = value;
                _arguments.Remove(nameof(CacheDirectory));
                return;
            }

            _cacheDirectory = value;

            var arguments = new[]
            {
                "--cache-dir",
                value
            };

            _arguments[nameof(CacheDirectory)] = arguments;
        }
    }
    private readonly string? _cacheDirectory;
        
    /// <summary>
    /// Path to a cert file for the certificate authority.
    /// </summary>
    public string? CertificateAuthority
    {
        get => _certificateAuthority;
        init
        {
            if (value is null)
            {
                _certificateAuthority = value;
                _arguments.Remove(nameof(CertificateAuthority));
                return;
            }

            _certificateAuthority = value;

            var arguments = new[]
            {
                "--certificate-authority",
                value
            };

            _arguments[nameof(CertificateAuthority)] = arguments;
        }
    }
    private readonly string? _certificateAuthority;
    
    /// <summary>
    /// Path to a client certificate file for TLS.
    /// </summary>
    public string? ClientCertificate
    {
        get => _clientCertificate;
        init
        {
            if (value is null)
            {
                _clientCertificate = value;
                _arguments.Remove(nameof(ClientCertificate));
                return;
            }

            _clientCertificate = value;

            var arguments = new[]
            {
                "--client-certificate",
                value
            };

            _arguments[nameof(ClientCertificate)] = arguments;
        }
    }
    private readonly string? _clientCertificate;
    
    /// <summary>
    /// Path to a client key file for TLS.
    /// </summary>
    public string? ClientKey
    {
        get => _clientKey;
        init
        {
            if (value is null)
            {
                _clientKey = value;
                _arguments.Remove(nameof(ClientKey));
                return;
            }

            _clientKey = value;

            var arguments = new[]
            {
                "--client-key",
                value
            };

            _arguments[nameof(ClientKey)] = arguments;
        }
    }
    private readonly string? _clientKey;
        
    /// <summary>
    /// The name of the kubeconfig cluster to use.
    /// </summary>
    public string? Cluster
    {
        // todo: default to whatever the current cluster is set to by the k8s client
        get => _cluster;
        init
        {
            if (value is null)
            {
                _cluster = value;
                _arguments.Remove(nameof(Cluster));
                return;
            }

            _cluster = value;

            var arguments = new[]
            {
                "--cluster",
                value
            };

            _arguments[nameof(Cluster)] = arguments;
        }
    } 
    private readonly string? _cluster;
        
    /// <summary>
    /// The name of the kubeconfig context to use. Defaults to the current context of the kubeconfig.
    /// </summary>
    public string Context
    {
        get => _context ??= KubernetesClientConfiguration.BuildConfigFromConfigFile().CurrentContext;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Context));

            _context = value;

            var arguments = new[]
            {
                "--context",
                value
            };

            _arguments[nameof(Context)] = arguments;
        }
    }
    private string? _context;
        
    /// <summary>
    /// If true, opt-out of response compression for all requests to the server.
    /// </summary>
    public bool? DisableCompression
    {
        get => _disableCompression;
        init
        {
            if (value is null)
            {
                _disableCompression = value;
                _arguments.Remove(nameof(DisableCompression));
                return;
            }

            _disableCompression = value;

            var arguments = new[]
            {
                "--disable-compression"
            };

            _arguments[nameof(DisableCompression)] = arguments;
        }
    }
    private readonly bool? _disableCompression;
        
    /// <summary>
    /// If true, the server's certificate will not be checked for validity.
    /// This will make your HTTPS connections insecure.
    /// </summary>
    public bool? InsecureSkipTlsVerify
    {
        get => _insecureSkipTlsVerify;
        init
        {
            if (value is null)
            {
                _insecureSkipTlsVerify = value;
                _arguments.Remove(nameof(InsecureSkipTlsVerify));
                return;
            }

            _insecureSkipTlsVerify = value;

            var arguments = new[]
            {
                "--insecure-skip-tls-verify"
            };

            _arguments[nameof(InsecureSkipTlsVerify)] = arguments;
        }
    }
    private readonly bool? _insecureSkipTlsVerify;
        
    /// <summary>
    /// Path to the kubeconfig file to use for CLI requests.
    /// </summary>
    public string? KubeConfig
    {
        get => _kubeConfig;
        init
        {
            if (value is null)
            {
                _kubeConfig = value;
                _arguments.Remove(nameof(KubeConfig));
                return;
            }

            _kubeConfig = value;

            var arguments = new[]
            {
                "--kubeconfig",
                value
            };

            _arguments[nameof(KubeConfig)] = arguments;
        }
    }
    private readonly string? _kubeConfig;
        
    /// <summary>
    /// The length of time to wait before giving up on a single server request.
    /// Non-zero values should contain a corresponding time unit (e.g. 1s, 2m, 3h).
    /// A value of zero means don't timeout requests. (default "0").
    /// </summary>
    public string? RequestTimeout
    {
        get => _requestTimeout;
        init
        {
            if (value is null)
            {
                _requestTimeout = value;
                _arguments.Remove(nameof(RequestTimeout));
                return;
            }

            _requestTimeout = value;

            var arguments = new[]
            {
                "--request-timeout",
                value
            };
            
            _arguments[nameof(RequestTimeout)] = arguments;
        }
    }
    private readonly string? _requestTimeout;
        
    /// <summary>
    /// The address and port of the Kubernetes API server.
    /// </summary>
    public string? Server
    {
        get => _server;
        init
        {
            if (value is null)
            {
                _server = value;
                _arguments.Remove(nameof(Server));
                return;
            }

            _server = value;

            var arguments = new[]
            {
                "--server",
                value
            };
            
            _arguments[nameof(Server)] = arguments;
        }
    }
    private readonly string? _server;
        
    /// <summary>
    /// Server name to use for server certificate validation.
    /// If it is not provided, the hostname used to contact the server is used.
    /// </summary>
    public string? TlsServerName
    {
        get => _tlsServerName;
        init
        {
            if (value is null)
            {
                _tlsServerName = value;
                _arguments.Remove(nameof(TlsServerName));
                return;
            }

            _tlsServerName = value;

            var arguments = new[]
            {
                "--tls-server-name",
                value
            };
            
            _arguments[nameof(TlsServerName)] = arguments;
        }
    }
    private readonly string? _tlsServerName;
        
    /// <summary>
    /// Bearer token for authentication to the API server.
    /// </summary>
    public string? Token
    {
        get => _token;
        init
        {
            if (value is null)
            {
                _token = value;
                _arguments.Remove(nameof(Token));
                return;
            }

            _token = value;

            var arguments = new[]
            {
                "--token",
                value
            };
            
            _arguments[nameof(Token)] = arguments;
        }
    }
    private readonly string? _token;
        
    /// <summary>
    /// The name of the kubeconfig user to use.
    /// </summary>
    public string? User
    {
        get => _user;
        init
        {
            if (value is null)
            {
                _user = value;
                _arguments.Remove(nameof(User));
                return;
            }

            _user = value;

            var arguments = new[]
            {
                "--user",
                value
            };
            
            _arguments[nameof(User)] = arguments;
        }
    }
    private readonly string? _user;
}