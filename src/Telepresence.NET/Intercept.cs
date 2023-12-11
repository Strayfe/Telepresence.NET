using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Telepresence.NET.Converters;
using Telepresence.NET.Models.Intercept;
using Telepresence.NET.Models.Intercept.Handlers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Telepresence.NET;

public class Intercept
{
    private readonly string _temporaryDirectory;
    private readonly string _interceptOutputPath;
    private readonly string _interceptSpecificationPath;
    
    private readonly string _name;
    // todo: private IEnumerable<Prerequisites> _prerequisites;
    private readonly Connection? _connection;
    private readonly IEnumerable<Workload>? _workloads;
    private readonly IEnumerable<Handler>? _handlers;
    
    private Process? _interceptProcess;

    public Intercept()
    {
        _name = Assembly
            .GetEntryAssembly()?
            .GetName()
            .Name?
            .Replace('.', '-')
            .Replace('_', '-')
            .ToLowerInvariant() ?? 
                throw new InvalidOperationException(Constants.Exceptions.CantDetermineName);
        
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), _name);
        _interceptOutputPath = Path.Combine(_temporaryDirectory, $"{_name}-output.json");
        _interceptSpecificationPath = Path.Combine(_temporaryDirectory, $"{_name}-intercept-specification.yaml");
        
        if (!Directory.Exists(_temporaryDirectory))
            Directory.CreateDirectory(_temporaryDirectory);

        _workloads = new List<Workload>
        {
            new()
            {
                Name = _name,
                Intercepts = new List<WorkloadIntercept>
                {
                    new()
                    {
                        Name = _name,
                        Handler = _name,
                        Service = _name
                    }
                }
            }
        };

        _handlers = new List<Handler>
        {
            new()
            {
                Name = _name,
                External = new External
                {
                    IsDocker = false,
                    OutputFormat = OutputFormat.Json,
                    OutputPath = _interceptOutputPath
                }
            }
        };
    }

    /// <summary>
    /// A name to give to the specification.
    /// </summary>
    public string Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new ArgumentNullException(nameof(Name));
            
            return _name;
        }
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

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
        get => _connection;
        init => _connection = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    /// <summary>
    /// Remote workloads that are intercepted, keyed by workload name.
    /// </summary>
    public IEnumerable<Workload>? Workloads
    {
        get => _workloads;
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!value.Any() || value.Count() > 32)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfWorkloadsDefined);

            _workloads = value;
        }
    }

    /// <summary>
    /// Local services running on the host machine that handle the intercepted services requests.
    /// </summary>
    public IEnumerable<Handler>? Handlers
    {
        get => _handlers;
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
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
    public async Task Connect()
    {
        // todo: skip if connect already called by the user
        // if (_connected)
        //     return;
        
        LogToConsole("Attempting to connect to cluster");
        
        if (_connection == null)
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

            if (_connection != null && !string.IsNullOrWhiteSpace(_connection.Context))
            {
                connectProcess.StartInfo.ArgumentList.Add("--context");
                connectProcess.StartInfo.ArgumentList.Add(_connection.Context);
            }
            
            if (_connection != null && !string.IsNullOrWhiteSpace(_connection.Namespace))
            {
                connectProcess.StartInfo.ArgumentList.Add("--namespace");
                connectProcess.StartInfo.ArgumentList.Add(_connection.Namespace);
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
            
            await connectProcess.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            LogToConsole($"Couldn't connect to telepresence, {ex}");
        }
    }
    
    /// <summary>
    /// Start the intercept.
    /// </summary>
    public async Task Start(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        
        await Connect();
        await Leave();
        
        await File.WriteAllTextAsync(_interceptSpecificationPath, this.ToString());
        
        LogToConsole($"Starting intercept: {_name}");

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

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter((int)timeout.Value.TotalMilliseconds);
            await WaitForOutputFile(cancellationTokenSource.Token);

            // if the file doesn't exist after (n) seconds then the intercept clearly didn't work, so just exit here
            if (!File.Exists(_interceptOutputPath))
            {
                await Leave();
                throw new InvalidOperationException(Constants.Exceptions.UnableToStartIntercept);
            }

            OutputLoader.LoadEnvironment(_interceptOutputPath);

            // known limitation: only works with first of collection for now
            var workload = _workloads?.FirstOrDefault();
            if (workload == null)
                throw new InvalidOperationException(Constants.Exceptions.NoWorkloadFound);

            var intercept = workload.Intercepts?.FirstOrDefault();
            if (intercept == null)
                throw new InvalidOperationException(Constants.Exceptions.NoWorkloadInterceptFound);
            
            // explicitly set the port for Kestrel to listen on because this may have come back from the cluster
            // todo: account for tls
            Environment.SetEnvironmentVariable("DOTNET_URLS", $"http://+:{intercept.LocalPort}");
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{intercept.LocalPort}");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            LogToConsole($"Intercept started: {_name}");
        }
        catch (Exception ex)
        {
            LogToConsole($"Error starting intercept: {ex}");
            throw;
        }
        finally
        {
            DeleteOutputFile();
        }
    }

    /// <summary>
    /// Leave the intercept.
    /// </summary>
    public async Task Leave()
    {
        LogToConsole($"Attempting to leave previous intercept: {_name}");

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
                        $"{_name}"
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

            await leaveIntercept.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            LogToConsole($"Couldn't leave intercept: {ex}");
        }
        finally
        {
            DeleteOutputFile();
        }
    }

    /// <summary>
    /// Disconnect from the traffic manager in the cluster.
    /// </summary>
    public async Task Quit(bool stopDaemons = false)
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

    private void DeleteOutputFile()
    {
        if (File.Exists(_interceptOutputPath))
            File.Delete(_interceptOutputPath);
    }

    private async Task WaitForOutputFile(CancellationToken cancellationToken)
    {
        LogToConsole("Waiting for output file");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (File.Exists(_interceptOutputPath))
                    return;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            LogToConsole("Operation timed out");
        }
        catch (OperationCanceledException)
        {
            LogToConsole("Operation timed out");
        }
        catch (Exception ex)
        {
            LogToConsole(ex.Message);
            throw;
        }
    }

    // todo: temporary - use proper logging
    private static void LogToConsole(string log) => Console.WriteLine($"[telepresence]: {log}");
}