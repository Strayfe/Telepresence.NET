using Newtonsoft.Json;

namespace Telepresence.NET.InterceptSpec.Models.Output;

public class InterceptResult
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("disposition")]
    public string? Disposition { get; set; } // this is probably an enum, I just dunno what all the values are yet

    [JsonProperty("workload_kind")]
    public string? WorkloadKind { get; set; } // this is probably an enum, I just dunno what all the values are yet

    [JsonProperty("target_host")]
    public string? TargetHost { get; set; }

    [JsonProperty("target_port")]
    public int? TargetPort { get; set; }

    [JsonProperty("service_port_id")]
    public string? ServicePortId { get; set; }

    [JsonProperty("environment")]
    public Dictionary<string, string>? Environment { get; set; }

    [JsonProperty("mount")]
    public MountResult? Mount { get; set; }

    [JsonProperty("filter_desc")]
    public string? FilterDescription { get; set; }

    [JsonProperty("http_filter")]
    public IEnumerable<string>? HttpFilters { get; set; }

    [JsonProperty("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonProperty("ingress")]
    public IngressResult? Ingress { get; set; }
}