using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;
using Telepresence.NET.Converters;
using Telepresence.NET.Models.Intercept;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Telepresence.NET;

/// <summary>
/// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
/// </summary>
public class Intercept
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public Intercept()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
    
    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public Intercept(string name)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        Name = name;
    }

    private string? _temporaryDirectory;
    private string? _interceptSpecificationPath;
    private bool _isConnected;
    private string? _name;
    // todo: private IEnumerable<Prerequisites> _prerequisites;
    private Connection? _connection;
    private IEnumerable<Workload>? _workloads;
    private IEnumerable<Handler>? _handlers;
    private Process? _interceptProcess;

    /// <summary>
    /// A name to give to the specification.
    /// </summary>
    public string Name
    {
        get => _name ??= Constants.Defaults.NormalizedEntryAssembly;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";
            
            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _name = value;
        }
    }

    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection? Connection
    {
        get => _connection ??= new Connection();
        init => _connection = value ?? throw new ArgumentNullException(nameof(Connection));
    }
    
    /// <summary>
    /// Remote workloads that are intercepted, keyed by workload name.
    /// </summary>
    public IEnumerable<Workload> Workloads
    {
        get => _workloads ??= new List<Workload>
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Workloads));

            if (!value.Any() || value.Count() > 32)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfWorkloadsDefined);

            _workloads = value;
        }
    }

    /// <summary>
    /// Local services running on the host machine that handle the intercepted services requests.
    /// </summary>
    public IEnumerable<Handler> Handlers
    {
        get => _handlers ??= new List<Handler>
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Handlers));

            if (!value.Any() || value.Count() > 64)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfHandlersDefined);
            
            // assert that each handler has at least one handler
            foreach (var handler in value)
            {
                var isDocker = handler.Docker != null;
                var isScript = handler.Script != null;
                var isExternal = handler.External != null;

                var mutuallyExclusive = isDocker ^ isScript ^ isExternal;
                
                if (!mutuallyExclusive)
                    throw new InvalidOperationException(Constants.Exceptions.MutuallyExclusiveHandlers);
            }
            
            _handlers = value;
        }
    }
    
    /// <summary>
    /// Connect to the traffic manager in the cluster.
    /// Automatically done if not already connected.
    /// </summary>
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        if (_isConnected)
            return;
        
        // todo: configure a timeout 
        // var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        
        _logger.Information("Attempting to connect to cluster");
        
        if (Connection == null)
            _logger.Information("Connection not defined, will try to determine from context");
        
        try
        {
            var connectProcess = new Process
            {
                StartInfo =
                {
                    FileName = "telepresence",
                    ArgumentList =
                    {
                        "connect"
                    },
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            if (Connection != null && !string.IsNullOrWhiteSpace(Connection.Context))
            {
                connectProcess.StartInfo.ArgumentList.Add("--context");
                connectProcess.StartInfo.ArgumentList.Add(Connection.Context);
            }
            
            if (Connection != null && !string.IsNullOrWhiteSpace(Connection.Namespace))
            {
                connectProcess.StartInfo.ArgumentList.Add("--namespace");
                connectProcess.StartInfo.ArgumentList.Add(Connection.Namespace);
            }
            
            connectProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    _logger.Information(args.Data);
                    return;
                }
                
                // todo: figure out why this reporting when it shouldn't..
                _logger.Information("Already connected");
            };
            
            connectProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    _logger.Information(args.Data);
            };

            connectProcess.Start();
            
            connectProcess.BeginOutputReadLine();
            connectProcess.BeginErrorReadLine();
            
            await connectProcess.WaitForExitAsync(cancellationToken);

            _isConnected = true;
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't connect to telepresence");
        }
    }
    
    /// <summary>
    /// Start the intercept.
    /// </summary>
    public async Task Start(CancellationToken cancellationToken = default)
    {
        await Connect(cancellationToken);
        await Leave(cancellationToken);
        await CreateInterceptSpecification(cancellationToken);
        
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        
        _logger.Information("Starting intercept: {Name}", Name);

        try
        {
            _interceptProcess = new Process
            {
                StartInfo =
                {
                    FileName = "telepresence",
                    ArgumentList =
                    {
                        "intercept",
                        "run",
                        $"{_interceptSpecificationPath}"
                    },
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            
            // known limitation: only works with first of collection for now
            var workload = Workloads.FirstOrDefault();
            if (workload == null)
                throw new InvalidOperationException(Constants.Exceptions.NoWorkloadFound);

            var intercept = workload.Intercepts?.FirstOrDefault();
            if (intercept == null)
                throw new InvalidOperationException(Constants.Exceptions.NoWorkloadInterceptFound);

            var handler = Handlers.FirstOrDefault();
            if (handler == null)
                throw new InvalidOperationException(Constants.Exceptions.NoHandlerFound);

            await handler.Handle(_interceptProcess, linkedTokenSource.Token);
            
            // explicitly set the port for Kestrel to listen on because this may have come back from the cluster
            // todo: account for tls
            // todo: find a more logical place for this if this is not it, maybe a strategy pattern for intercepts too?
            Environment.SetEnvironmentVariable("DOTNET_URLS", $"http://+:{intercept.LocalPort}");
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{intercept.LocalPort}");
            
            _logger.Information("Intercept started: {Name}", Name);
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Error starting intercept");
            throw;
        }
    }

    /// <summary>
    /// Leave the intercept.
    /// </summary>
    public async Task Leave(CancellationToken cancellationToken = default)
    {
        var workload = Workloads.FirstOrDefault();
        var intercept = workload?.Intercepts?.FirstOrDefault();
        
        _logger.Information("Attempting to leave previous intercept: {Name}", intercept?.Name ?? Name);

        try
        {
            var leaveIntercept = new Process
            {
                StartInfo =
                {
                    FileName = "telepresence",
                    ArgumentList =
                    {
                        "leave",
                        $"{intercept?.Name ?? Name}"
                    },
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            leaveIntercept.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    _logger.Information(args.Data);
            };
            leaveIntercept.ErrorDataReceived += (sender, args) =>
            {
                // expected behaviour is that the intercept may not exist when trying to leave, this is fine
                if (args.Data != null && args.Data.Contains("not found"))
                {
                    _logger.Information("No previous intercept found");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(args.Data))
                    _logger.Information(args.Data);
            };

            leaveIntercept.Start();

            leaveIntercept.BeginOutputReadLine();
            leaveIntercept.BeginErrorReadLine();

            await leaveIntercept.WaitForExitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't leave intercept");
        }
    }

    /// <summary>
    /// Disconnect from the traffic manager in the cluster.
    /// </summary>
    public async Task Quit(bool stopDaemons = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

        DestroyAssets();
    }
    
    /// <summary>
    /// Clean up created resources.
    /// </summary>
    private void DestroyAssets()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, true);
    }
    
    /// <summary>
    /// Parse the intercept specification as YAML.
    /// Falls back to the name of the object in event of failure.
    /// </summary>
    public override string? ToString()
    {
        var result = base.ToString();

        try
        {
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            result = serializer.Serialize(this);
        }
        catch (Exception)
        {
            // ignored
        }

        return result;
    }
    
    private Task CreateInterceptSpecification(CancellationToken cancellationToken = default)
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), Name);
        _interceptSpecificationPath = Path.Combine(_temporaryDirectory, $"{Name}-intercept-specification.yaml");
        
        if (!Directory.Exists(_temporaryDirectory))
            Directory.CreateDirectory(_temporaryDirectory);
        
        return File.WriteAllTextAsync(_interceptSpecificationPath, ToString(), cancellationToken);
    }
}