using System.Runtime.Serialization;

namespace Telepresence.NET.InterceptSpec.Handlers;

internal enum Shell
{
    [EnumMember(Value = "bash")] Bash,
    [EnumMember(Value = "zsh")] Zsh,
    [EnumMember(Value = "sh")] Sh,
}