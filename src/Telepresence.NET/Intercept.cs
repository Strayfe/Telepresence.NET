using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;
using Telepresence.NET.Converters;
using Telepresence.NET.Models;
using Telepresence.NET.Models.Intercept;
using Telepresence.NET.Models.Intercept.Handlers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Telepresence.NET;

/// <summary>
/// An intercept and processes to control intercepting workloads in a kubernetes cluster.
/// </summary>
public class Intercept
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// An intercept and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public Intercept()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public Intercept(InterceptSpecification interceptSpecification)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        // todo: do something with the intercept specification
    }

    private string? _temporaryDirectory;
    private string? _interceptSpecificationPath;
    private bool _isConnected;
    private Process? _interceptProcess;
    private readonly Collection<string> _arguments = [];
    
    /// <summary>
    /// Local address to forward to, Only accepts IP address as a value. e.g. '10.0.0.2' (default "127.0.0.1")
    /// </summary>
    public string? Address
    {
        get => _arguments.FirstOrDefault(x => x.Contains("--address"));
        init
        {
            if (value is null)
                return;
            
            // todo: sanity check for well-formed IP addresses
            _arguments.Add($"--address {value}"); 
        }
    }
    
    /// <summary>
    /// Provide very detailed info about the intercept when used together with 
    /// </summary>
    public OutputFormat? DetailedOutput { get; init; }
    
    /// <summary>
    /// Build a Docker container from the given docker-context (path or URL), and run it with intercepted environment and volume
    /// mounts, by passing arguments after -- to 'docker run', e.g. '--docker-build /path/to/docker/context -- -it IMAGE /bin/bash'
    /// </summary>
    public string? DockerBuild { get; init; }
    
    /// <summary>
    /// Options to docker-build in the form of key value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? DockerBuildOptions { get; init; }
    
    /// <summary>
    /// Like <see cref="DockerBuild"/>, but allows a debugger to run inside the container with relaxed security
    /// </summary>
    public string? DockerDebug { get; init; }
    
    /// <summary>
    /// The volume mount point in docker. Defaults to same as "<see cref="Mount"/>
    /// </summary>
    public string? DockerMount { get; init; }
    
    /// <summary>
    /// Run a Docker container with intercepted environment, volume mount, by passing arguments after -- to 'docker run', e.g.
    /// '--docker-run -- -it --rm ubuntu:20.04 /bin/bash'
    /// </summary>
    public bool? DockerRun { get; init; }
    
    /// <summary>
    /// Also emit the remote environment to an env file in Docker Compose format. See https://docs.docker.com/compose/env-file/ for
    /// more information on the limitations of this format.
    /// </summary>
    public string? EnvFile { get; init; }
    
    /// <summary>
    /// Also emit the remote environment to a file as a JSON blob.
    /// </summary>
    public string? EnvJson { get; init; }
    
    /// <summary>
    /// Only intercept traffic that matches this "HTTP2_HEADER=REGEXP" specifier. Instead of a "--http-header=HTTP2_HEADER=REGEXP"
    /// pair, you may say "--http-header=auto", which will automatically select a unique matcher for your intercept. If this flag is
    /// given multiple times, then it will only intercept traffic that matches *all* of the specifiers. (default [auto])
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? HttpHeader { get; init; }
    
    /// <summary>
    /// Associates key=value pairs with the intercept that can later be retrieved using the Telepresence API service. (implies
    /// "--mechanism=http")
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>>? HttpMeta { get; init; }
    
    /// <summary>
    /// Only intercept traffic with paths that are exactly equal to this path once the query string is removed. (implies
    /// "--mechanism=http")
    /// </summary>
    public string? HttpPathEqual { get; init; }
    
    /// <summary>
    /// Only intercept traffic with paths beginning with this prefix. (implies "--mechanism=http")
    /// </summary>
    public string? HttpPathPrefix { get; init; }
    
    /// <summary>
    /// Only intercept traffic with paths that are entirely matched by this regular expression once the query string is removed.
    /// (implies "--mechanism=http")
    /// </summary>
    public string? HttpPathRegexp { get; init; }
    
    /// <summary>
    /// Use plaintext format when communicating with the interceptor process on the local workstation. Only meaningful when
    /// intercepting workloads annotated with "getambassador.io/inject-originating-tls-secret" to prevent that TLS is used during
    /// intercepts. (implies "--mechanism=http")
    /// </summary>
    public bool? HttpPlainText { get; init; }
    
    /// <summary>
    /// The ingress hostname.
    /// </summary>
    public string? IngressHost { get; init; }
    
    /// <summary>
    /// The L5 hostname. Defaults to the ingress-host value
    /// </summary>
    public string? IngressL5Host { get; init; }
    
    /// <summary>
    /// The ingress port.
    /// </summary>
    public int? IngressPort { get; init; }
    
    /// <summary>
    /// Determines if TLS is used.
    /// </summary>
    public bool? IngressTls { get; init; }
    
    /// <summary>
    /// Do not mount remote directories. Instead, expose this port on localhost to an external mounter
    /// </summary>
    public ushort? LocalMountPort { get; init; }
    
    /// <summary>
    /// Which extension mechanism to use (default tcp)
    /// </summary>
    public Mechanism? Mechanism { get; init; }
    
    /// <summary>
    /// The absolute path for the root directory where volumes will be mounted, $TELEPRESENCE_ROOT. Use "true" to have Telepresence
    /// pick a random mount point (default). Use "false" to disable filesystem mounting entirely. (default "true")
    /// </summary>
    public string? Mount { get; init; }
    
    /// <summary>
    /// Local port to forward to. If intercepting a service with multiple ports, use <local port>:<svcPortIdentifier>, where the
    /// identifier is the port name or port number. With --docker-run and a daemon that doesn't run in docker', use <local
    /// port>:<container port> or <local port>:<container port>:<svcPortIdentifier>.
    /// </summary>
    public string? Port { get; init; }
    public string? PreviewUrl { get; init; }
    public string? PreviewUrlAddRequestHeaders { get; init; }
    public string? PreviewUrlBanner { get; init; }
    public string? Replace { get; init; }
    public string? Service { get; init; }
    public string? ToPod { get; init; }
    public string? UseSavedIntercept { get; init; }
    public string? Workload { get; init; }

    /// <summary>
    /// Connect to the traffic manager in the cluster.
    /// Automatically done if not already connected.
    /// </summary>
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        if (_isConnected)
            return;
        
        // todo: configure a timeout 
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        
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
            
            await connectProcess.WaitForExitAsync(linkedTokenSource.Token);

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
    // todo: try to automatically determine the license rather than asking the user to tell us
    public async Task Start(License license, CancellationToken cancellationToken = default)
    {
        await Connect(cancellationToken);
        await Leave(cancellationToken);
        
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        
        _logger.Information("Starting intercept: {Name}", Name);

        try
        {
            switch (license)
            {
                case License.Community:
                    _interceptProcess = CreateInterceptCommandLineInterfaceProcess();
                    break;
                case License.Developer:
                case License.Enterprise:
                    _interceptProcess = await CreateInterceptSpecificationProcess(cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(license), license, null);
            }
            
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
    
    private Process CreateInterceptCommandLineInterfaceProcess()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "telepresence",
            ArgumentList =
            {
                "intercept",
            },
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in _arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        
        return new Process
        {
            StartInfo = startInfo
        };
    }

    private async Task<Process> CreateInterceptSpecificationProcess(CancellationToken cancellationToken = default)
    {
        await CreateInterceptSpecificationFile(cancellationToken);
        
        return new Process
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
    }

    private Task CreateInterceptSpecificationFile(CancellationToken cancellationToken = default)
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), Name);
        _interceptSpecificationPath = Path.Combine(_temporaryDirectory, $"{Name}-intercept-specification.yaml");
        
        if (!Directory.Exists(_temporaryDirectory))
            Directory.CreateDirectory(_temporaryDirectory);
        
        return File.WriteAllTextAsync(_interceptSpecificationPath, ToString(), cancellationToken);
    }
}