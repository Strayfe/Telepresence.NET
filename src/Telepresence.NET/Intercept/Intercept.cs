using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;

namespace Telepresence.NET.Intercept;

/// <summary>
/// Intercept a process.
/// </summary>
public partial class Intercept
{
    private readonly ILogger _logger;

    /// <summary>
    /// Intercept a process.
    /// </summary>
    public Intercept()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Intercept a process.
    /// </summary>
    public Intercept(string name)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Name = name;
    }

    private readonly Dictionary<string, IEnumerable<string>> _arguments = [];

    /// <summary>
    /// Intercept base name.
    /// </summary>
    public string Name
    {
        get => _name ?? Constants.Defaults.NormalizedEntryAssembly;
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
    private readonly string? _name;

    /// <summary>
    /// Start the intercept using the provided settings.
    /// </summary>
    public async Task Start(CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        // we need to try leaving so that we can get a new env file
        if (InjectEnvironment is true) 
            await Leave(linkedTokenSource.Token);

        _logger.Information("Attempting to start intercept");

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
                        Name
                    },
                    WorkingDirectory = System.Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            
            foreach (var argument in _arguments.Values.SelectMany(argument => argument))
                startProcess.StartInfo.ArgumentList.Add(argument);

            _logger.Information($"executing command: telepresence {string.Join(" ", startProcess.StartInfo.ArgumentList)}");

            startProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data)) 
                    _logger.Information(args.Data);
            };

            startProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    _logger.Information(args.Data);
            };

            startProcess.Start();

            startProcess.BeginOutputReadLine();
            startProcess.BeginErrorReadLine();

            await startProcess.WaitForExitAsync(linkedTokenSource.Token);

            if (InjectEnvironment is true)
            {
                _logger.Information("Attempting to automatically inject environment");

                // todo: neaten up and reduce cognitive complexity
                if (!string.IsNullOrWhiteSpace(EnvFile))
                {
                    _logger.Information("Waiting for file to become ready");
                    await WaitForOutputFile(EnvFile, linkedTokenSource.Token);

                    if (!File.Exists(EnvFile))
                        throw new InvalidOperationException(Constants.Exceptions.UnableToStartIntercept);

                    await EnvironmentLoader.LoadEnvironmentFromFile(EnvFile, linkedTokenSource.Token);

                    // apply manual overrides
                    if (IncludeEnvironment != null && Enumerable.Any<KeyValuePair<string, string>>(IncludeEnvironment))
                    {
                        if (ExcludeEnvironment == null || !Enumerable.Any<string>(ExcludeEnvironment))
                            await EnvironmentLoader.SetEnvironmentVariables(IncludeEnvironment);
                        else
                        {
                            var includeSansExclude = Enumerable
                                .Where<KeyValuePair<string, string>>(IncludeEnvironment, x => !Enumerable.Contains<string>(ExcludeEnvironment, x.Key));

                            await EnvironmentLoader.SetEnvironmentVariables(includeSansExclude);
                        }
                    }

                    // todo: handle strange issue on Windows where the output file is locked by the telepresence.exe
                    //       for about 90 seconds so it cannot be deleted, maybe we need to use a file watcher that
                    //       deletes it as soon as it unlocks
                    if (File.Exists(EnvFile))
                        File.Delete(EnvFile);
                }
                else if (!string.IsNullOrWhiteSpace(EnvJson))
                {
                    _logger.Information("Waiting for file to become ready");
                    await WaitForOutputFile(EnvJson, linkedTokenSource.Token);

                    if (!File.Exists(EnvJson))
                        throw new InvalidOperationException(Constants.Exceptions.UnableToStartIntercept);

                    await EnvironmentLoader.LoadEnvironmentFromFile(EnvJson, linkedTokenSource.Token);

                    // apply manual overrides
                    if (IncludeEnvironment != null && Enumerable.Any<KeyValuePair<string, string>>(IncludeEnvironment))
                    {
                        if (ExcludeEnvironment == null || !Enumerable.Any<string>(ExcludeEnvironment))
                            await EnvironmentLoader.SetEnvironmentVariables(IncludeEnvironment);
                        else
                        {
                            var includeSansExclude = Enumerable
                                .Where<KeyValuePair<string, string>>(IncludeEnvironment, x => !Enumerable.Contains<string>(ExcludeEnvironment, x.Key));

                            await EnvironmentLoader.SetEnvironmentVariables(includeSansExclude);
                        }
                    }

                    _logger.Information("Environment loaded");
                    
                    // todo: handle strange issue on Windows where the output file is locked by the telepresence.exe
                    //       for about 90 seconds so it cannot be deleted, maybe we need to use a file watcher that
                    //       deletes it as soon as it unlocks
                    if (File.Exists(EnvJson))
                        File.Delete(EnvJson);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't start intercept");
        }
    }

    /// <summary>
    /// Leave the intercept.
    /// </summary>
    public async Task Leave(CancellationToken cancellationToken = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        _logger.Information("Attempting to leave intercept");

        try
        {
            var leaveProcess = new Process
            {
                StartInfo =
                {
                    FileName = "telepresence",
                    ArgumentList =
                    {
                        "leave",
                        Name
                    },
                    WorkingDirectory = System.Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            // todo: create global argument list and separate out from _arguments
            if (!string.IsNullOrWhiteSpace(Use))
            {
                leaveProcess.StartInfo.ArgumentList.Add("--use");
                leaveProcess.StartInfo.ArgumentList.Add(Use);
            }

            _logger.Information($"executing command: telepresence {string.Join(" ", leaveProcess.StartInfo.ArgumentList)}");

            leaveProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data)) 
                    _logger.Information(args.Data);
            };

            leaveProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    _logger.Information(args.Data);
            };

            leaveProcess.Start();

            leaveProcess.BeginOutputReadLine();
            leaveProcess.BeginErrorReadLine();

            await leaveProcess.WaitForExitAsync(linkedTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.Information(ex, "Couldn't leave intercept");
        }
    }

    private static async Task WaitForOutputFile(string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (File.Exists(outputPath))
                    return;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }
}