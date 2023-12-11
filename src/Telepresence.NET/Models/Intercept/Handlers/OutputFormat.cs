using System.Runtime.Serialization;

namespace Telepresence.NET.Models.Intercept.Handlers;

public enum OutputFormat
{
    [EnumMember(Value = "json")] Json,
    [EnumMember(Value = "yaml")] Yaml
}