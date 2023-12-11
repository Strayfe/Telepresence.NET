using System.Runtime.Serialization;

namespace Telepresence.NET.Models.Intercept.Handlers;

public enum Shell
{
    [EnumMember(Value = "bash")] Bash,
    [EnumMember(Value = "zsh")] Zsh,
    [EnumMember(Value = "sh")] Sh,
}