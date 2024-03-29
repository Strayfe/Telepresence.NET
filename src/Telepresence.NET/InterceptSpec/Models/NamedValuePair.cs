namespace Telepresence.NET.InterceptSpec.Models;

#nullable disable
public class NamedValuePair<TName, TValue>
{
    public TName Name { get; init; }
    public TValue Value { get; init; }
}