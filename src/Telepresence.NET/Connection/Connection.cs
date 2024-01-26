using System.Diagnostics;
using Serilog;

namespace Telepresence.NET.Connection;

/// <summary>
/// Connect to a cluster.
/// </summary>
public partial class Connection
{
    private readonly ILogger _logger;

    /// <summary>
    /// Connect to a cluster.
    /// </summary>
    public Connection()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Connection to a cluster.
    /// </summary>
    public Connection(string name)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Name = name;
    }

    private bool _connected;
    private readonly Dictionary<string, IEnumerable<string>> _arguments = [];

    /// <summary>
    /// Connect to the traffic manager in the cluster.
    /// Automatically done if not already connected.
    /// </summary>
    public async Task Connect(CancellationToken cancellationToken = default)
    {
        if (_connected)
            return;

        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        _logger.Information("Attempting to connect");

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

            foreach (var argument in _arguments.Values.SelectMany(argument => argument))
                connectProcess.StartInfo.ArgumentList.Add(argument);

            _logger.Information($"executing command: telepresence {string.Join(" ", connectProcess.StartInfo.ArgumentList)}");
            
            connectProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data)) 
                    _logger.Information(args.Data);
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

            _connected = true;
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't connect to telepresence");
        }
    }

    /// <summary>
    /// Disconnect from the cluster without stopping the daemons.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task Disconnect(CancellationToken cancellationToken = default) => 
        await Quit(false, cancellationToken);

    /// <summary>
    /// Fully stop the Telepresence process and daemons.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task Quit(CancellationToken cancellationToken = default) => 
        await Quit(true, cancellationToken);

    private async Task Quit(bool stopDaemons, CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        _logger.Information("Attempting to disconnect");

        try
        {
            var connectProcess = new Process
            {
                StartInfo =
                {
                    FileName = "telepresence",
                    ArgumentList =
                    {
                        "quit"
                    },
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            if (stopDaemons)
                connectProcess.StartInfo.ArgumentList.Add("--stop-daemons");

            _logger.Information($"executing command: telepresence {string.Join(" ", connectProcess.StartInfo.ArgumentList)}");

            connectProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data)) 
                    _logger.Information(args.Data);
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

            _connected = true;
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't connect to telepresence");
        }
    }
}