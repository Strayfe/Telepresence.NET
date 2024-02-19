using System.Text.RegularExpressions;

namespace Telepresence.NET.Intercept;

/// <summary>
/// The intercept options that will be used via the CLI based mechanism for establishing an intercept.
/// </summary>
public partial class Intercept
{
    /// <summary>
    /// Local address to forward to, Only accepts IP address as a value. e.g. '10.0.0.2' (default "127.0.0.1")
    /// </summary>
    // there is currently a naming mismatch between the intercept specification and the cli so a breaking change may
    // be coming soon to rename this to `LocalAddress`
    public string? Address
    {
        get => _address;
        init
        {
            if (value is null)
            {
                _address = value;
                _arguments.Remove(nameof(Address));
                return;
            }

            const string pattern = @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$|\{\{";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.NotAnIpAddress);

            _address = value;

            var arguments = new[]
            {
                "--address",
                value
            };

            _arguments[nameof(Address)] = arguments;
        }
    }
    private readonly string? _address;

    /// <summary>
    /// Build a Docker container from the given docker-context (path or URL), and run it with intercepted environment and volume
    /// mounts, by passing arguments after -- to 'docker run', e.g. '--docker-build /path/to/docker/context -- -it IMAGE /bin/bash'
    /// </summary>
    public string? DockerBuild
    {
        get => _dockerBuild;
        init
        {
            if (value is null)
            {
                _dockerBuild = value;
                _arguments.Remove(nameof(DockerBuild));
                return;
            }

            _dockerBuild = value;

            var arguments = new[]
            {
                "--docker-build",
                value
            };

            _arguments[nameof(DockerBuild)] = arguments;
        }
    }
    private readonly string? _dockerBuild;

    /// <summary>
    /// Options to docker-build in the form of key value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? DockerBuildOptions
    {
        get => _dockerBuildOptions;
        init
        {
            if (value is null)
            {
                _dockerBuildOptions = value;
                _arguments.Remove(nameof(DockerBuildOptions));
                return;
            }

            if (DockerBuild is null)
                throw new ArgumentNullException(nameof(DockerBuild));

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(DockerBuildOptions));

            _dockerBuildOptions = value;

            var arguments = new List<string>();

            foreach (var val in value.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
            {
                arguments.Add("--docker-build-opt");
                arguments.Add($"{val.Key}={val.Value}");
            }

            _arguments[nameof(DockerBuildOptions)] = arguments;
        }
    }
    private readonly IEnumerable<KeyValuePair<string, string>>? _dockerBuildOptions;

    /// <summary>
    /// Like <see cref="DockerBuild"/>, but allows a debugger to run inside the container with relaxed security
    /// </summary>
    public string? DockerDebug
    {
        get => _dockerDebug;
        init
        {
            if (value is null)
            {
                _dockerDebug = value;
                _arguments.Remove(nameof(DockerDebug));
                return;
            }

            _dockerDebug = value;

            var arguments = new[]
            {
                "--docker-debug",
                value
            };

            _arguments[nameof(DockerDebug)] = arguments;
        }
    }
    private readonly string? _dockerDebug;

    /// <summary>
    /// The volume mount point in docker. Defaults to same as "<see cref="Mount"/>
    /// </summary>
    public string? DockerMount
    {
        get => _dockerMount;
        init
        {
            if (value is null)
            {
                _dockerMount = value;
                _arguments.Remove(nameof(DockerMount));
                return;
            }

            _dockerMount = value;

            var arguments = new[]
            {
                "--docker-mount",
                value
            };

            _arguments[nameof(DockerMount)] = arguments;
        }
    }
    private readonly string? _dockerMount;

    /// <summary>
    /// Run a Docker container with intercepted environment, volume mount, by passing arguments after -- to 'docker run', e.g.
    /// '--docker-run -- -it --rm ubuntu:20.04 /bin/bash'
    /// </summary>
    public string? DockerRun
    {
        get => _dockerRun;
        init
        {
            if (value is null)
            {
                _dockerRun = value;
                _arguments.Remove(nameof(DockerRun));
                return;
            }

            _dockerRun = value;

            var arguments = new[]
            {
                "--docker-run",
                value
            };

            _arguments[nameof(DockerRun)] = arguments;
        }
    }
    private readonly string? _dockerRun;

    /// <summary>
    /// Also emit the remote environment to an env file in Docker Compose format.
    /// See https://docs.docker.com/compose/env-file/ for more information on the limitations of this format.
    /// </summary>
    public string? EnvFile
    {
        get => _envFile;
        init
        {
            if (value is null)
            {
                _envFile = value;
                _arguments.Remove(nameof(EnvFile));
                return;
            }

            _envFile = value;

            var arguments = new[]
            {
                "--env-file",
                value
            };

            _arguments[nameof(EnvFile)] = arguments;
        }
    }
    private readonly string? _envFile;

    /// <summary>
    /// Also emit the remote environment to a file as a JSON blob.
    /// </summary>
    public string? EnvJson
    {
        get => _envJson;
        init
        {
            if (value is null)
            {
                _envJson = value;
                _arguments.Remove(nameof(EnvJson));
                return;
            }

            _envJson = value;

            var arguments = new[]
            {
                "--env-json",
                value
            };

            _arguments[nameof(EnvJson)] = arguments;
        }
    }
    private readonly string? _envJson;

    /// <summary>
    /// Only intercept traffic that matches this "HTTP2_HEADER=REGEXP" specifier. Instead of a "--http-header=HTTP2_HEADER=REGEXP"
    /// pair, you may say "--http-header=auto", which will automatically select a unique matcher for your intercept. If this flag is
    /// given multiple times, then it will only intercept traffic that matches *all* of the specifiers. (default [auto])
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? HttpHeader
    {
        get => _httpHeader;
        init
        {
            if (value is null)
            {
                _httpHeader = value;
                _arguments.Remove(nameof(HttpHeader));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(HttpHeader));

            _httpHeader = value;

            var arguments = new List<string>();

            foreach (var val in value.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
            {
                arguments.Add("--http-header");
                arguments.Add($"{val.Key}={val.Value}");
            }

            _arguments[nameof(HttpHeader)] = arguments;
        }
    }
    private readonly IEnumerable<KeyValuePair<string, string>>? _httpHeader;

    /// <summary>
    /// Associates key=value pairs with the intercept that can later be retrieved using the Telepresence API service.
    /// </summary>
    /// <remarks>
    /// (implies "--mechanism=http")
    /// </remarks>
    public IEnumerable<KeyValuePair<string, string>>? HttpMeta
    {
        get => _httpMeta;
        init
        {
            if (value is null)
            {
                _httpMeta = value;
                _arguments.Remove(nameof(HttpMeta));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(HttpMeta));

            _httpMeta = value;

            var arguments = new List<string>();

            foreach (var val in value.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
            {
                arguments.Add("--http-meta");
                arguments.Add($"{val.Key}={val.Value}");
            }

            _arguments[nameof(HttpMeta)] = arguments;
        }
    }
    private readonly IEnumerable<KeyValuePair<string, string>>? _httpMeta;

    /// <summary>
    /// Only intercept traffic with paths that are exactly equal to this path once the query string is removed. (implies
    /// "--mechanism=http")
    /// </summary>
    public string? HttpPathEqual
    {
        get => _httpPathEqual;
        init
        {
            if (value is null)
            {
                _httpPathEqual = value;
                _arguments.Remove(nameof(HttpPathEqual));
                return;
            }

            _httpPathEqual = value;

            var arguments = new[]
            {
                "--http-path-equal",
                value
            };

            _arguments[nameof(HttpPathEqual)] = arguments;
        }
    }
    private readonly string? _httpPathEqual;

    /// <summary>
    /// Only intercept traffic with paths beginning with this prefix. (implies "--mechanism=http")
    /// </summary>
    public string? HttpPathPrefix
    {
        get => _httpPathPrefix;
        init
        {
            if (value is null)
            {
                _httpPathPrefix = value;
                _arguments.Remove(nameof(HttpPathPrefix));
                return;
            }

            _httpPathPrefix = value;

            var arguments = new[]
            {
                "--http-path-prefix",
                value
            };

            _arguments[nameof(HttpPathPrefix)] = arguments;
        }
    }
    private readonly string? _httpPathPrefix;

    /// <summary>
    /// Only intercept traffic with paths that are entirely matched by this regular expression once the query string is removed.
    /// (implies "--mechanism=http")
    /// </summary>
    public string? HttpPathRegexp
    {
        get => _httpPathRegexp;
        init
        {
            if (value is null)
            {
                _httpPathRegexp = value;
                _arguments.Remove(nameof(HttpPathRegexp));
                return;
            }

            _httpPathRegexp = value;

            var arguments = new[]
            {
                "--http-path-regexp",
                value
            };

            _arguments[nameof(HttpPathRegexp)] = arguments;
        }
    }
    private readonly string? _httpPathRegexp;

    /// <summary>
    /// Use plaintext format when communicating with the interceptor process on the local workstation. Only meaningful when
    /// intercepting workloads annotated with "getambassador.io/inject-originating-tls-secret" to prevent that TLS is used during
    /// intercepts. (implies "--mechanism=http")
    /// </summary>
    public bool? HttpPlainText
    {
        get => _httpPlainText;
        init
        {
            if (value is null)
            {
                _httpPlainText = value;
                _arguments.Remove(nameof(HttpPlainText));
                return;
            }

            _httpPlainText = value;

            var arguments = new[]
            {
                "--http-plaintext"
            };

            _arguments[nameof(HttpPlainText)] = arguments;
        }
    }
    private readonly bool? _httpPlainText;

    /// <summary>
    /// The ingress hostname.
    /// </summary>
    public string? IngressHost
    {
        get => _ingressHost;
        init
        {
            if (value is null)
            {
                _ingressHost = value;
                _arguments.Remove(nameof(IngressHost));
                return;
            }

            _ingressHost = value;

            var arguments = new[]
            {
                "--ingress-host",
                value
            };

            _arguments[nameof(IngressHost)] = arguments;
        }
    }
    private readonly string? _ingressHost;

    /// <summary>
    /// The L5 hostname. Defaults to the ingress-host value
    /// </summary>
    public string? IngressL5Host
    {
        get => _ingressL5Host;
        init
        {
            if (value is null)
            {
                _ingressL5Host = value;
                _arguments.Remove(nameof(IngressL5Host));
                return;
            }

            _ingressL5Host = value;

            var arguments = new[]
            {
                "--ingress-l5",
                value
            };

            _arguments[nameof(IngressL5Host)] = arguments;
        }
    }
    private readonly string? _ingressL5Host;

    /// <summary>
    /// The ingress port.
    /// </summary>
    public int? IngressPort
    {
        get => _ingressPort;
        init
        {
            if (value is null)
            {
                _ingressPort = value;
                _arguments.Remove(nameof(IngressPort));
                return;
            }

            // todo: might need to do some more validation for valid port numbers here

            _ingressPort = value;

            var arguments = new[]
            {
                "--ingress-port",
                value.ToString()!
            };

            _arguments[nameof(IngressPort)] = arguments;
        }
    }
    private readonly int? _ingressPort;

    /// <summary>
    /// Determines if TLS is used.
    /// </summary>
    public bool? IngressTls
    {
        get => _ingressTls;
        init
        {
            if (value is null)
            {
                _ingressTls = value;
                _arguments.Remove(nameof(IngressTls));
                return;
            }

            _ingressTls = value;

            var arguments = new[]
            {
                "--ingress-tls"
            };

            _arguments[nameof(IngressTls)] = arguments;
        }
    }
    private readonly bool? _ingressTls;

    /// <summary>
    /// Do not mount remote directories. Instead, expose this port on localhost to an external mounter
    /// </summary>
    public ushort? LocalMountPort
    {
        get => _localMountPort;
        init
        {
            if (value is null)
            {
                _localMountPort = value;
                _arguments.Remove(nameof(LocalMountPort));
                return;
            }

            // todo: might need to do some more validation for valid port numbers here

            _localMountPort = value;

            var arguments = new[]
            {
                "--local-mount-port",
                value.ToString()!
            };

            _arguments[nameof(LocalMountPort)] = arguments;
        }
    }
    private readonly ushort? _localMountPort;

    /// <summary>
    /// Which extension mechanism to use (default tcp)
    /// </summary>
    public Mechanism? Mechanism
    {
        get => _mechanism;
        init
        {
            if (value is null)
            {
                _mechanism = value;
                _arguments.Remove(nameof(Mechanism));
                return;
            }

            _mechanism = value;

            var arguments = new[]
            {
                "--mechanism",
                value.ToString()!
            };

            _arguments[nameof(Mechanism)] = arguments;
        }
    }
    private readonly Mechanism? _mechanism;

    /// <summary>
    /// The absolute path for the root directory where volumes will be mounted, $TELEPRESENCE_ROOT. Use "true" to have Telepresence
    /// pick a random mount point (default). Use "false" to disable filesystem mounting entirely. (default "true")
    /// </summary>
    public string? Mount
    {
        get => _mount;
        init
        {
            if (value is null)
            {
                _mount = value;
                _arguments.Remove(nameof(Mount));
                return;
            }

            _mount = value;

            var arguments = new[]
            {
                "--mount",
                value
            };

            _arguments[nameof(Mount)] = arguments;
        }
    }
    private readonly string? _mount;

    /// <summary>
    /// Local port to forward to.
    /// If intercepting a service with multiple ports, use &lt;local port&gt;:&lt;svcPortIdentifier&gt;, where the
    /// identifier is the port name or port number.
    /// With --docker-run and a daemon that doesn't run in docker', use &lt;local port&gt;:&lt;container port&gt; or
    /// &lt;local port&gt;:&lt;container port&gt;:&lt;svcPortIdentifier&gt;.
    /// </summary>
    public string? Port
    {
        get => _port;
        init
        {
            if (value is null)
            {
                _port = value;
                _arguments.Remove(nameof(Port));
                return;
            }

            _port = value;

            var arguments = new[]
            {
                "--port",
                value
            };

            _arguments[nameof(Port)] = arguments;
        }
    }
    private readonly string? _port;

    /// <summary>
    /// Generate an edgestack.me preview domain for this intercept. (default true)
    /// </summary>
    public bool? PreviewUrl
    {
        get => _previewUrl;
        init
        {
            if (value is null)
            {
                _previewUrl = value;
                _arguments.Remove(nameof(PreviewUrl));
                return;
            }

            _previewUrl = value;

            var arguments = new[]
            {
                "--preview-url"
            };

            _arguments[nameof(PreviewUrl)] = arguments;
        }
    }
    private readonly bool? _previewUrl;

    /// <summary>
    /// Additional headers in key1=value1,key2=value2 pairs injected in every preview page request (default [])
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? PreviewUrlAddRequestHeaders
    {
        get => _previewUrlAddRequestHeaders;
        init
        {
            if (value is null)
            {
                _previewUrlAddRequestHeaders = value;
                _arguments.Remove(nameof(PreviewUrlAddRequestHeaders));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(PreviewUrlAddRequestHeaders));

            _previewUrlAddRequestHeaders = value;

            var pairs = value
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(pair => $"{pair.Key}={pair.Value}")
                .ToList();

            var headers = string.Join(',', pairs);

            var arguments = new[]
            {
                "--preview-url-add-request-headers",
                headers
            };

            _arguments[nameof(PreviewUrlAddRequestHeaders)] = arguments;
        }
    }
    private readonly IEnumerable<KeyValuePair<string, string>>? _previewUrlAddRequestHeaders;

    /// <summary>
    /// Display banner on preview page (default true)
    /// </summary>
    public bool? PreviewUrlBanner
    {
        get => _previewUrlBanner;
        init
        {
            if (value is null)
            {
                _previewUrlBanner = value;
                _arguments.Remove(nameof(PreviewUrlBanner));
                return;
            }

            _previewUrlBanner = value;

            var arguments = new[]
            {
                "--preview-url-banner"
            };

            _arguments[nameof(PreviewUrlBanner)] = arguments;
        }
    }
    private readonly bool? _previewUrlBanner;

    /// <summary>
    /// Whether to replace the running container entirely. If false, the app container will keep running.
    /// </summary>
    public bool? Replace
    {
        get => _replace;
        init
        {
            if (value is null)
            {
                _replace = value;
                _arguments.Remove(nameof(Replace));
                return;
            }

            _replace = value;

            var arguments = new[]
            {
                "--replace"
            };

            _arguments[nameof(Replace)] = arguments;
        }
    }
    private readonly bool? _replace;

    /// <summary>
    /// Name of service to intercept. If not provided, we will try to auto-detect one
    /// </summary>
    public string? Service
    {
        get => _service;
        init
        {
            if (value is null)
            {
                _service = value;
                _arguments.Remove(nameof(Service));
                return;
            }

            _service = value;

            var arguments = new[]
            {
                "--service",
                value
            };

            _arguments[nameof(Service)] = arguments;
        }
    }
    private readonly string? _service;

    /// <summary>
    /// An additional port to forward from the intercepted pod, will be made available at localhost:PORT Use this to,
    /// for example, access proxy/helper sidecars in the intercepted pod.
    /// The default protocol is TCP. Use &lt;port&gt;/UDP for UDP ports
    /// </summary>
    public IEnumerable<string>? ToPod
    {
        get => _toPod;
        init
        {
            if (value is null)
            {
                _toPod = value;
                _arguments.Remove(nameof(ToPod));
                return;
            }

            if (!value.Any())
                throw new ArgumentOutOfRangeException(nameof(ToPod));

            _toPod = value;

            var arguments = new List<string>();

            foreach (var port in value.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                arguments.Add("--to-pod");
                arguments.Add(port);
            }

            _arguments[nameof(ToPod)] = arguments;
        }
    }
    private readonly IEnumerable<string>? _toPod;

    /// <summary>
    /// Use a saved intercept.
    /// </summary>
    public string? UseSavedIntercept
    {
        get => _useSavedIntercept;
        init
        {
            if (value is null)
            {
                _useSavedIntercept = value;
                _arguments.Remove(nameof(UseSavedIntercept));
                return;
            }

            _useSavedIntercept = value;

            var arguments = new[]
            {
                "--use-saved-intercept",
                value
            };

            _arguments[nameof(UseSavedIntercept)] = arguments;
        }
    }
    private readonly string? _useSavedIntercept;

    /// <summary>
    /// Name of workload (Deployment, ReplicaSet) to intercept, if different from &lt;name&gt;
    /// </summary>
    public string? Workload
    {
        get => _workload;
        init
        {
            if (value is null)
            {
                _workload = value;
                _arguments.Remove(nameof(Workload));
                return;
            }

            _workload = value;

            var arguments = new[]
            {
                "--workload",
                value
            };

            _arguments[nameof(Workload)] = arguments;
        }
    }
    private readonly string? _workload;

    // todo: move to a global mechanism??
    /// <summary>
    /// Match expression that uniquely identified the daemon container.
    /// </summary>
    public string? Use
    {
        get => _use;
        init
        {
            if (value is null)
            {
                _use = value;
                _arguments.Remove(nameof(Use));
                return;
            }

            _use = value;

            var arguments = new[]
            {
                "--use",
                value
            };

            _arguments[nameof(Use)] = arguments;
        }
    }
    private readonly string? _use;
}