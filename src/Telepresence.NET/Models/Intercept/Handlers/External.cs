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

    /// <summary>
    /// 
    /// </summary>
    public async Task Handle(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
            return;

        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

        await WaitForOutputFile(OutputPath, linkedTokenSource.Token);
    
        if (!File.Exists(OutputPath))
            throw new InvalidOperationException(Exceptions.UnableToStartIntercept);
    
        OutputLoader.LoadEnvironment(OutputPath);
                
        if (File.Exists(OutputPath))
            File.Delete(OutputPath);
    }
    
    private static async Task WaitForOutputFile(string outputPath, CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (File.Exists(outputPath))
                {
                    try
                    {
                        // check to see if the file can be opened to read the contents
                        await using var stream = File.Open(outputPath, FileMode.Open, FileAccess.Read);
                        return;
                    }
                    catch (IOException)
                    {
                        // file is not yet readable so just continue the loop until it becomes readable or times out
                    }
                }

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