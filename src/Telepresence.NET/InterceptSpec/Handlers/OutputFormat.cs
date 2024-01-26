using System.Runtime.Serialization;

namespace Telepresence.NET.InterceptSpec.Handlers;

internal enum OutputFormat
{
    [EnumMember(Value = "json")] Json,
    [EnumMember(Value = "yaml")] Yaml
}