namespace Telepresence.NET.HeaderPropagation;

/// <summary>
/// A tracking context injected into the DI container that facilitates header propagation.
/// </summary>
public sealed class TelepresenceContext
{
    public IDictionary<string, string> InterceptHeaders { get; } = new Dictionary<string, string>();
    // todo: track optional path
    // todo: track metadata
}
