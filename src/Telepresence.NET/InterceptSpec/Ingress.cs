namespace Telepresence.NET.InterceptSpec;

internal class Ingress
{
    private readonly int? _port;

    /// <summary>
    /// The ingress hostname.
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    /// The ingress L5 Hostname.
    /// Defaults to ingressHost.
    /// </summary>
    public string? L5Host { get; init; }

    /// <summary>
    /// The ingress port.
    /// </summary>
    public int? Port
    {
        get => _port;
        init
        {
            if (value is < 1 or > 65535)
                throw new InvalidOperationException(Constants.Exceptions.NotValidPort);

            _port = value;
        }
    }

    /// <summary>
    /// Determines if TLS is used.
    /// </summary>
    public bool Tls { get; init; }
}