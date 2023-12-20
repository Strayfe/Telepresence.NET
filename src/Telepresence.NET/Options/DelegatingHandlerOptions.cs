namespace Telepresence.NET.Options;

public class DelegatingHandlerOptions
{
    /// <summary>
    /// <para>
    /// A list of header names to look to propagate to downstream requests.
    /// </para>
    /// <para>
    /// This should match any custom HTTP headers specified in the Intercept Specification.
    /// </para>
    /// <para>
    /// Defaults to standards defined by the default Intercept Specification.
    /// </para>
    /// </summary>
    public IEnumerable<string> InterceptHeaderNames { get; set; } = Array.Empty<string>();
}