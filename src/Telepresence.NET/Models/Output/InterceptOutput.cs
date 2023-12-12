using Newtonsoft.Json;

namespace Telepresence.NET.Models.Output;

public class InterceptOutput
{
    [JsonProperty("name")]
    public string? Name { get; set; }
    
    [JsonProperty("intercepts")]
    public IEnumerable<InterceptResult>? Intercepts { get; set; }
    
    // strange object, looks like a directory but behaves like an object, not really c# friendly without reflection so skip for now
    // [JsonProperty("bindMounts")]
    // public object? BindMounts { get; set; }
    
    [JsonProperty("environment")] public IDictionary<string, string>? Environment { get; set; }
}