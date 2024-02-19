using System.Diagnostics;
using Telepresence.NET.InterceptSpec.Handlers.ExternalHandlers;

namespace Telepresence.NET.InterceptSpec.Handlers;

/// <summary>
/// Output info to external runner.
/// </summary>
internal class External : IHandlerStrategy
{
    private readonly IExternalHandlerStrategy? _externalHandlerStrategy;
    private readonly string? _outputPath;

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
    public string? OutputPath {
        get => _outputPath;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (string.Equals("stdout", value, StringComparison.OrdinalIgnoreCase))
            {
                _outputPath = value;
                _externalHandlerStrategy = new StandardOutputHandler();
                return;
            }

            if (string.Equals("stderr", value, StringComparison.OrdinalIgnoreCase))
            {
                _outputPath = value;
                _externalHandlerStrategy = new StandardErrorHandler();
                return;
            }

            // check it is a valid path, specific exceptions will be thrown if not
            _outputPath = Path.GetFullPath(value);
            _externalHandlerStrategy = new FileOutputHandler
            {
                OutputPath = value
            };
        }
    }

    public async Task Handle(Process process, CancellationToken cancellationToken = default) =>
        await _externalHandlerStrategy.Handle(process, cancellationToken);
}