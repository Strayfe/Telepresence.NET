using Newtonsoft.Json;
using Telepresence.NET.InterceptSpec.Models.Output;

namespace Telepresence.NET.InterceptSpec;

internal static class OutputLoader
{
    public static async Task LoadEnvironmentFromString(string outputString, CancellationToken cancellationToken = default) =>
        await ProcessJsonOutput(outputString, cancellationToken);

    /// <summary>
    /// Loads environment variables from an output file into the currently running process.
    /// </summary>
    public static async Task LoadEnvironmentFromFile(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return;

        var extension = Path.GetExtension(filePath);

        var processors = new Dictionary<string, Func<string, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            [".json"] = ProcessJson,
            [".yml"] = ProcessYaml,
            [".yaml"] = ProcessYaml,
            [".env"] = ProcessDotEnv,
        };

        if (processors.TryGetValue(extension, out var processor))
            await processor(filePath, cancellationToken);
    }

    private static async Task ProcessJsonOutput(string output, CancellationToken cancellationToken = default)
    {
        var interceptOutput = JsonConvert.DeserializeObject<InterceptOutput>(output);

        if (interceptOutput == null)
            return;

        // get environment from individual intercepts (limited to first for now)
        var firstIntercept = interceptOutput.Intercepts?.FirstOrDefault();

        if (firstIntercept?.Environment is { Count: > 0 })
            SetEnvironmentVariables(firstIntercept.Environment);

        // apply environment overrides applied directly to intercept specification
        if (interceptOutput.Environment is { Count: > 0 })
            SetEnvironmentVariables(interceptOutput.Environment);
    }

    private static async Task ProcessJson(string filePath, CancellationToken cancellationToken = default)
    {
        await WaitForRead(filePath, cancellationToken);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var interceptOutput = JsonConvert.DeserializeObject<InterceptOutput>(json);

        if (interceptOutput == null)
            return;

        // get environment from individual intercepts (limited to first for now)
        var firstIntercept = interceptOutput.Intercepts?.FirstOrDefault();

        if (firstIntercept?.Environment is { Count: > 0 })
            SetEnvironmentVariables(firstIntercept.Environment);

        // apply environment overrides applied directly to intercept specification
        if (interceptOutput.Environment is { Count: > 0 })
            SetEnvironmentVariables(interceptOutput.Environment);
    }

    private static Task ProcessYaml(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // this is a bit rudimentary, more processing may be required to handle empty variables, commented variables, etc.
    private static Task ProcessDotEnv(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }

        return Task.CompletedTask;
    }

    private static async Task WaitForRead(string filePath, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var inputStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return;
            }
            catch (IOException)
            {
                // loop until file becomes readable with sharable lock or times out
            }
        }
    }

    private static void SetEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> environments)
    {
        foreach (var environment in environments)
            Environment.SetEnvironmentVariable(environment.Key, environment.Value);
    }
}