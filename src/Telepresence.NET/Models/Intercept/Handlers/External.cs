namespace Telepresence.NET.Models.Intercept.Handlers;

/// <summary>
/// Output info to external runner.
/// </summary>
public class External : IHandlerStrategy
{
    /// <summary>
    /// True if the external runner is containerized.
    /// </summary>
    public bool? IsDocker { get; init; }
    
    /// <summary>
    /// Format for the emitted info.
    /// </summary>
    public OutputFormat? OutputFormat { get; init; }
    
    /// <summary>
    /// Path to the file that will receive the output.
    /// Can be stdout, stderr, or a file path.
    /// </summary>
    public string? OutputPath { get; init; }
    
    // public async Task Handle(CancellationToken cancellationToken = default)
    // {
    //     if (string.IsNullOrWhiteSpace(OutputPath))
    //         return;
    //
    //     var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    //     linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
    //
    //     await WaitForOutputFile(OutputPath, linkedTokenSource.Token);
    //
    //     if (!File.Exists(OutputPath))
    //         throw new InvalidOperationException(Exceptions.UnableToStartIntercept);
    //
    //     // reset the timeout
    //     linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
    //     
    //     await OutputLoader.LoadEnvironment(OutputPath, linkedTokenSource.Token);
    //
    //     // due to issues on windows that need further testing, we cannot delete the output file because telepresence
    //     // seems to hold a lock on the file for about 90 seconds after its finished writing
    //     // if (File.Exists(OutputPath))
    //     //     File.Delete(OutputPath);
    // }

    public async Task Handle(string output, CancellationToken cancellationToken = default)
    {
        if (string.Equals("stdout", OutputPath, StringComparison.OrdinalIgnoreCase))
        {
            await OutputLoader.LoadEnvironment(output, cancellationToken);
            return;
        }

        if (string.Equals("stderr", OutputPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotImplementedException();
        }
        
        // if its a file path, throw for now
        throw new NotImplementedException();
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