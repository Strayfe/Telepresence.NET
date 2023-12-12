using System.Diagnostics;
using System.Text.RegularExpressions;
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
    private string? _temporaryDirectory;
    private string? _interceptSpecificationPath;
    private bool _isConnected;
    private string? _name;
    // todo: private IEnumerable<Prerequisites> _prerequisites;
    private readonly Connection? _connection;
    private IEnumerable<Workload>? _workloads;
    private IEnumerable<Handler>? _handlers;
    
    private Process? _interceptProcess;

    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public Intercept()
    {
    }

    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public Intercept(string name) => Name = name;

    /// <summary>
    /// A name to give to the specification.
    /// </summary>
    public string Name
    {
        get => _name ??= Defaults.Name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";
            
            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Exceptions.AlphaNumericWithHyphens);

            _name = value;
        }
    }

    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection? Connection
    {
        get => _connection;
        init => _connection = value ?? throw new ArgumentNullException(nameof(Connection));
    }
    
    /// <summary>
    /// Remote workloads that are intercepted, keyed by workload name.
    /// </summary>
    public IEnumerable<Workload>? Workloads
    {
        get => _workloads ??= new Workload[]
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Workloads));

            if (!value.Any() || value.Count() > 32)
                throw new InvalidOperationException(Exceptions.InvalidNumberOfWorkloadsDefined);

            _workloads = value;
        }
    }

    /// <summary>
    /// Local services running on the host machine that handle the intercepted services requests.
    /// </summary>
    public IEnumerable<Handler>? Handlers
    {
        get => _handlers ??= new Handler[]
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Handlers));

            if (!value.Any() || value.Count() > 64)
                throw new InvalidOperationException(Exceptions.InvalidNumberOfHandlersDefined);
            
            // assert that each handler has at least one handler
            foreach (var handler in value)
            {
                var isDocker = handler.Docker != null;
                var isScript = handler.Script != null;
                var isExternal = handler.External != null;

                var mutuallyExclusive = isDocker ^ isScript ^ isExternal;
                
                if (!mutuallyExclusive)
                    throw new InvalidOperationException(Exceptions.MutuallyExclusiveHandlers);
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
        
        LogToConsole("Attempting to connect to cluster");
        
        if (Connection == null)
            LogToConsole("Connection not defined, will try to determine from context");
        
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
                    LogToConsole(args.Data);
                    return;
                }
                
                LogToConsole("Already connected");
            };
            
            connectProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    LogToConsole(args.Data);
            };

            connectProcess.Start();
            
            connectProcess.BeginOutputReadLine();
            connectProcess.BeginErrorReadLine();
            
            await connectProcess.WaitForExitAsync(cancellationToken);

            _isConnected = true;
        }
        catch (Exception ex)
        {
            LogToConsole($"Couldn't connect to telepresence, {ex}");
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
        
        LogToConsole($"Starting intercept: {Name}");

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

            _interceptProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    LogToConsole(args.Data);
            };
            
            _interceptProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    LogToConsole(args.Data);
            };

            _interceptProcess.Start();
            
            _interceptProcess.BeginOutputReadLine();
            _interceptProcess.BeginErrorReadLine();
            
            // known limitation: only works with first of collection for now
            var workload = Workloads?.FirstOrDefault();
            if (workload == null)
                throw new InvalidOperationException(Exceptions.NoWorkloadFound);

            var intercept = workload.Intercepts?.FirstOrDefault();
            if (intercept == null)
                throw new InvalidOperationException(Exceptions.NoWorkloadInterceptFound);

            var handler = Handlers?.FirstOrDefault();
            if (handler == null)
                throw new InvalidOperationException(Exceptions.NoHandlerFound);

            await handler.Handle(cancellationToken);
            
            // explicitly set the port for Kestrel to listen on because this may have come back from the cluster
            // todo: account for tls
            // todo: find a more logical place for this if this is not it, maybe a strategy pattern for intercepts too?
            Environment.SetEnvironmentVariable("DOTNET_URLS", $"http://+:{intercept.LocalPort}");
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{intercept.LocalPort}");
            
            LogToConsole($"Intercept started: {Name}");
        }
        catch (Exception ex)
        {
            LogToConsole($"Error starting intercept: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Leave the intercept.
    /// </summary>
    public async Task Leave(CancellationToken cancellationToken = default)
    {
        LogToConsole($"Attempting to leave previous intercept: {Name}");

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
                        $"{Name}"
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
                    LogToConsole(args.Data);
            };
            leaveIntercept.ErrorDataReceived += (sender, args) =>
            {
                // expected behaviour is that the intercept may not exist when trying to leave, this is fine
                if (args.Data != null && args.Data.Contains("not found"))
                {
                    LogToConsole("No previous intercept found");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(args.Data))
                    LogToConsole(args.Data);
            };

            leaveIntercept.Start();

            leaveIntercept.BeginOutputReadLine();
            leaveIntercept.BeginErrorReadLine();

            await leaveIntercept.WaitForExitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogToConsole($"Couldn't leave intercept: {ex}");
        }
    }

    /// <summary>
    /// Disconnect from the traffic manager in the cluster.
    /// </summary>
    public async Task Quit(bool stopDaemons = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// Clean up created resources.
    /// </summary>
    public void DestroyAssets()
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

    // todo: temporary - use proper logging
    private static void LogToConsole(string log) => Console.WriteLine($"[telepresence]: {log}");

    private Task CreateInterceptSpecification(CancellationToken cancellationToken = default)
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), Name);
        _interceptSpecificationPath = Path.Combine(_temporaryDirectory, $"{Name}-intercept-specification.yaml");
        
        if (!Directory.Exists(_temporaryDirectory))
            Directory.CreateDirectory(_temporaryDirectory);
        
        return File.WriteAllTextAsync(_interceptSpecificationPath, ToString(), cancellationToken);
    }
}