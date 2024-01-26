using System.Diagnostics;
using Serilog;
using Serilog.Core;

namespace Telepresence.NET.InterceptSpec.Handlers.ExternalHandlers;

internal class StandardOutputHandler : IExternalHandlerStrategy
{
    private readonly Logger _logger = new LoggerConfiguration()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    public async Task Handle(Process process, CancellationToken cancellationToken = default)
    {
        var outputWaiter = new TaskCompletionSource<bool>();
            
        process.OutputDataReceived += async (sender, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data))
                return;
        
            if (args.Data.StartsWith('{') && args.Data.Contains("\"environment\":"))
            {
                var outputString = args.Data[..(args.Data.LastIndexOf('}') + 1)];
                
                await OutputLoader.LoadEnvironmentFromString(outputString, cancellationToken);
                
                outputWaiter.SetResult(true);
                
                return;
            }
            
            _logger.Information(args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
                _logger.Information(args.Data);
        };

        process.Start();
            
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await outputWaiter.Task;
    }
}