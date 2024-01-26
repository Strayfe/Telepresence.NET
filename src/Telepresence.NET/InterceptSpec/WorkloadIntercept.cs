using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Telepresence.NET.Models;
using YamlDotNet.Serialization;

namespace Telepresence.NET.InterceptSpec;

/// <summary>
/// The services and/or ports to intercept
/// </summary>
internal class WorkloadIntercept
{
    private string? _handler;
    private string? _name;
    private int? _localPort;
    private readonly string? _localAddress;
    private string? _service;
    private readonly int? _port;
    private readonly bool? _global;
    private IEnumerable<NamedValuePair<string, string>>? _headers;

    /// <summary>
    /// The services and/or ports to intercept
    /// </summary>
    public WorkloadIntercept()
    {
    }

    /// <summary>
    /// The services and/or ports to intercept
    /// </summary>
    public WorkloadIntercept(string name) => Name = name;
    
    /// <summary>
    /// If set to false, disabled this intercept.
    /// Default is true.
    /// </summary>
    public bool? Enabled { get; init; } = true;

    /// <summary>
    /// The intercept handler that handles this intercept.
    /// </summary>
    public string? Handler
    {
        get => _handler ??= Name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Handler));
            
            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
            
            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _handler = value;
        }
    }

    /// <summary>
    /// Optional name of the intercept.
    /// </summary>
    public string? Name
    {
        get => _name ??= Constants.Defaults.NormalizedEntryAssembly;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            
            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
            
            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _name = value;
        }
    }

    /// <summary>
    /// The local port that will receive the intercepted traffic.
    /// If not specified, one will be created automatically between 55000 - 65535.
    /// </summary>
    public int? LocalPort
    {
        get => _localPort ??= GenerateRandomLocalPort();
        init
        {
            if (value == null)
                return;

            if (value is < 1 or > 65535)
                throw new InvalidOperationException(Constants.Exceptions.NotValidPort);

            _localPort = (int)value;
        }
    }

    /// <summary>
    /// The local IP address that will receive the intercepted traffic.
    /// </summary>
    public string? LocalAddress
    {
        get => _localAddress;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            
            const string pattern = @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.NotAnIpAddress);

            _localAddress = value;
        }
    }
    
    /// <summary>
    /// The local directory or drive where the remote volumes are mounted.
    /// </summary>
    public string? MountPoint { get; init; }

    /// <summary>
    /// Name of service to intercept
    /// </summary>
    public string? Service
    {
        get => _service ??= Name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            
            const string pattern = "^[a-z][a-z0-9-]{1,62}$";
            
            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _service = value;
        }
    }

    /// <summary>
    /// The port that will be intercepted.
    /// </summary>
    public int? Port
    {
        get => _port;
        init
        {
            if (value == null)
                return;
            
            if (value is < 1 or > 65535)
                throw new InvalidOperationException(Constants.Exceptions.NotValidPort);

            _port = value;
        }
    }

    /// <summary>
    /// If true, then intercept all tcp/udp traffic.
    /// Mutually exclusive with headers and path properties.
    /// </summary>
    public bool? Global
    {
        get => _global;
        init
        {
            if (value == null)
                return;
            
            if (_headers != null ||
                !string.IsNullOrWhiteSpace(PathPrefix) ||
                !string.IsNullOrWhiteSpace(PathSuffix) ||
                !string.IsNullOrWhiteSpace(PathEqual) ||
                !string.IsNullOrWhiteSpace(PathRegexp))
                throw new InvalidOperationException(Constants.Exceptions.GlobalMutuallyExclusive);

            _global = value;
        } 
    }

    /// <summary>
    /// If true, replace the running container.
    /// Default is false.
    /// </summary>
    public bool? Replace { get; init; }

    /// <summary>
    /// Headers that will filter the intercept.
    /// If omitted or empty, and if global isn't explicitly set to true, an auto header will be generated.
    /// </summary>
    public IEnumerable<NamedValuePair<string, string>>? Headers
    {
        get => _headers ??= GenerateDefaultHeaders();
        init
        {
            if (value == null || !value.Any())
                return;

            _headers = value;
        }
    }
    
    // /// <summary>
    // /// Metadata that will be returned by the Telepresence API
    // /// </summary>
    // public IEnumerable<object> Meta { get; init; }

    /// <summary>
    /// Path prefix filter for the intercept.
    /// Defaults to "/".
    /// </summary>
    public string? PathPrefix { get; init; }
    
    /// <summary>
    /// Path suffix filter for the intercept.
    /// </summary>
    public string? PathSuffix { get; init; }
    
    /// <summary>
    /// Path filter for the intercept.
    /// </summary>
    public string? PathEqual { get; init; }
    
    /// <summary>
    /// Path regular expression filter for the intercept.
    /// </summary>
    public string? PathRegexp { get; init; }
    
    /// <summary>
    /// Use plaintext format when communicating with the interceptor process on the local workstation.
    /// Only meaningful when intercepting workloads annotated with "getambassador.io/inject-originating-tls-secret" to
    /// prevent that TLS is used during intercepts.
    /// </summary>
    public bool? Plaintext { get; init; }
    
    /// <summary>
    /// Configures the Preview URL generated by Telepresence Enterprise.
    /// </summary>
    [YamlMember(Alias = "previewURL")] public PreviewUrl? PreviewUrl { get; init; }

    private static int GenerateRandomLocalPort()
    {
        var random = new Random();
        int port;
        
        while (true)
        {
            port = random.Next(55000, 65535);
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipGlobalProperties.GetActiveTcpConnections();

            if (tcpConnections.Any(x => x.LocalEndPoint.Port == port))
                continue;
            
            return port;
        }
    }

    private static IEnumerable<NamedValuePair<string, string>> GenerateDefaultHeaders() =>
    [
        new NamedValuePair<string, string>
        {
            Name = Constants.Defaults.Headers.TelepresenceInterceptAs,
            Value = Environment.UserName
            // seems to be broken on windows currently, doesn't seem to escape backslashes, will raise to ambassador
            // "{{ .Telepresence.Username }}"
        }
    ];
}