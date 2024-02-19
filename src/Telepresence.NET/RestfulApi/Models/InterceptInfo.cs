using Newtonsoft.Json;

namespace Telepresence.NET.RestfulApi.Models;

public sealed class InterceptInfo
{
    [JsonProperty("intercepted")]
    public bool Intercepted { get; init; }

    [JsonProperty("clientSide")]
    public bool ClientSide { get; init; }

    [JsonProperty("metadata")]
    public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}