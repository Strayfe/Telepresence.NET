using Newtonsoft.Json;

namespace Telepresence.NET.InterceptSpec.Models.Output;

public class IngressResult
{
    [JsonProperty("host")]
    public string? Host { get; init; }

    [JsonProperty("port")]
    public int? Port { get; init; }

    [JsonProperty("l5host")]
    public string? Layer5Host { get; init; }
}