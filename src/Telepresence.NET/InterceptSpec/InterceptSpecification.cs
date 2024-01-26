using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;
using Telepresence.NET.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Telepresence.NET.InterceptSpec;

/// <summary>
/// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
/// </summary>
internal class InterceptSpecification
{
    private readonly ILogger _logger;

    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public InterceptSpecification()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public InterceptSpecification(string name)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        Name = name;
    }

    private string? _temporaryDirectory;
    private string? _interceptSpecificationPath;

    // todo: private IEnumerable<Prerequisites> _prerequisites;

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
    
            const string pattern = @"^[a-zA-Z][a-zA-Z0-9_-]*$|\{\{";
            
            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);
    
            _name = value;
        }
    }
    private string? _name;
    
    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection? Connection
    {
        get => _connection ??= new Connection();
        init => _connection = value ?? throw new ArgumentNullException(nameof(Connection));
    }
    private Connection? _connection;
    
    /// <summary>
    /// Remote workloads that are intercepted, keyed by workload name.
    /// </summary>
    public IEnumerable<Workload> Workloads
    {
        get => _workloads ??= new List<Workload>
        {
            new() { Name = Name }
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
    private IEnumerable<Workload>? _workloads;
    
    /// <summary>
    /// Local services running on the host machine that handle the intercepted services requests.
    /// </summary>
    public IEnumerable<Handler> Handlers
    {
        get => _handlers ??= new List<Handler>
        {
            new() { Name = Name }
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
    private IEnumerable<Handler>? _handlers;
    
    /// <summary>
    /// Start an intercept based on an intercept specification.
    /// </summary>
    public async Task Run(CancellationToken cancellationToken = default)
    { 
        // should we be opinionated and automatically connect and leave existing?
        await CreateInterceptSpecificationFile(cancellationToken);

        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        _logger.Information("Starting intercept: {Name}", Name);

        try
        {
            var startProcess = new Process
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

            await handler.Handle(startProcess, linkedTokenSource.Token);
            
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
    /// Leave an intercept.
    /// </summary>
    public async Task Leave(CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        
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

            await leaveIntercept.WaitForExitAsync(linkedTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't leave intercept");
        }
        finally
        {
            DestroyAssets();
        }
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

    private Task CreateInterceptSpecificationFile(CancellationToken cancellationToken = default)
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), Name);
        _interceptSpecificationPath = Path.Combine(_temporaryDirectory, $"{Name}-intercept-specification.yaml");
        
        if (!Directory.Exists(_temporaryDirectory))
            Directory.CreateDirectory(_temporaryDirectory);
        
        return File.WriteAllTextAsync(_interceptSpecificationPath, ToString(), cancellationToken);
    }
    
    /// <summary>
    /// Clean up created resources.
    /// </summary>
    private void DestroyAssets()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, true);
    }
}