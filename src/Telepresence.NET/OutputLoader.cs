using System.Reflection;
using Newtonsoft.Json;
using Telepresence.NET.Models.Output;

namespace Telepresence.NET;

public static class OutputLoader
{
    private static string? InterceptName => Assembly
        .GetEntryAssembly()?
        .GetName()
        .Name?
        .Replace('.', '-')
        .Replace('_', '-')
        .ToLowerInvariant();
    
    // load environment based on convention
    public static void LoadEnvironment()
    {
        var tempPath = Path.GetTempPath();
        var interceptSpecificationPath = Path.Combine(tempPath, $"{InterceptName}.json");
        
        LoadEnvironment(interceptSpecificationPath);
    }
    
    // load environment from a specific file
    public static void LoadEnvironment(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var extension = Path.GetExtension(filePath);

        var processors = new Dictionary<string, Action<string>>
        {
            { ".json", ProcessJson },
            { ".yml", ProcessYaml },
            { ".yaml", ProcessYaml },
            { ".env", ProcessDotEnv }
        };

        if (processors.ContainsKey(extension)) 
            processors[extension](filePath);
    }
    
    private static void ProcessJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var interceptOutput = JsonConvert.DeserializeObject<InterceptOutput>(json);

        if (interceptOutput == null)
            return;
        
        // get environment from individual intercepts (limited to first for now)
        var intercept = interceptOutput
            .Intercepts?
            .FirstOrDefault(x => string.Equals(x.Name, InterceptName, StringComparison.OrdinalIgnoreCase));

        if (intercept is { Environment: not null } && intercept.Environment.Any())
        {
            foreach (var environment in intercept.Environment)
                Environment.SetEnvironmentVariable(environment.Key, environment.Value);
        }

        // apply environment overrides applied directly to intercept specification
        if (interceptOutput.Environment == null || !interceptOutput.Environment.Any())
            return;
        
        foreach (var environment in interceptOutput.Environment)
                Environment.SetEnvironmentVariable(environment.Key, environment.Value);
    }

    private static void ProcessYaml(string filePath)
    {
        throw new NotImplementedException();
    }

    // this is a bit rudimentary, more processing may be required to handle empty variables, commented variables, etc.
    private static void ProcessDotEnv(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
    
            if (parts.Length != 2)
                continue;
    
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}