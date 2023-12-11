namespace Telepresence.NET.Models.Intercept.Handlers;

/// <summary>
/// Output info to external runner.
/// </summary>
public class External
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
}