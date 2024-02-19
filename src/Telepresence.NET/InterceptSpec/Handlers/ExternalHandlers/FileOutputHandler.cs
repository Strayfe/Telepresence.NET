using System.Diagnostics;

namespace Telepresence.NET.InterceptSpec.Handlers.ExternalHandlers;

internal class FileOutputHandler : IExternalHandlerStrategy
{
    public string? OutputPath { get; init; }

    public async Task Handle(Process process, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
            return;

        await WaitForOutputFile(OutputPath, cancellationToken);

        if (!File.Exists(OutputPath))
            throw new InvalidOperationException(Constants.Exceptions.UnableToStartIntercept);

        await OutputLoader.LoadEnvironmentFromFile(OutputPath, cancellationToken);

        // todo: handle strange issue on Windows where the output file is locked by the telepresence.exe
        //       for about 90 seconds so it cannot be deleted, maybe we need to use a file watcher that
        //       deletes it as soon as it unlocks
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