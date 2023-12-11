namespace Telepresence.NET.Models.Intercept.Handlers;

/// <summary>
/// Run a script using a shell.
/// </summary>
public class Script
{
    private readonly string? _run;

    public string? Run
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_run))
                throw new ArgumentNullException(nameof(Run));

            return _run;
        }
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            _run = value;
        }
    }

    public Shell? Shell { get; init; }
}