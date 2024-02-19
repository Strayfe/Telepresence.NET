using Newtonsoft.Json;

namespace Telepresence.NET.InterceptSpec.Models.Output;

public class MountResult
{
    [JsonProperty("remote_dir")]
    public string? RemoteDirectory { get; init; }

    [JsonProperty("pod_ip")]
    public string? PodIpAddress { get; init; }

    [JsonProperty("port")]
    public int? Port { get; init; }

    [JsonProperty("mounts")]
    public IEnumerable<string>? Mounts { get; init; }

    [JsonProperty("error")]
    public string? Error { get; init; }
}