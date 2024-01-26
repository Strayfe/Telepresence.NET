using Newtonsoft.Json;

namespace Telepresence.NET.Intercept;

public static class EnvironmentLoader
{
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
    
    public static Task SetEnvironmentVariables(IEnumerable<KeyValuePair<string,string>> variables)
    {
        foreach (var variable in variables)
            Environment.SetEnvironmentVariable(variable.Key, variable.Value);

        return Task.CompletedTask;
    }

    private static async Task ProcessJson(string filePath, CancellationToken cancellationToken = default)
    {
        await WaitForRead(filePath, cancellationToken);
        
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var environment = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        if (environment == null)
            return;

        await SetEnvironmentVariables(environment);
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
}