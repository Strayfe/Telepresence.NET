using System.Text.RegularExpressions;
using Telepresence.NET.Models.Intercept.Handlers;

namespace Telepresence.NET.Models.Intercept;

/// <summary>
/// The resource where the intercepted requests will be routed to, i.e. a running version of the code on your machine.
/// </summary>
public class Handler
{
    private string? _name;
    private readonly IEnumerable<NamedValuePair<string, string>>? _environment;
    private External? _external;

    /// <summary>
    /// The resource where the intercepted requests will be routed to, i.e. a running version of the code on your machine.
    /// </summary>
    public Handler()
    {
    }

    /// <summary>
    /// The resource where the intercepted requests will be routed to, i.e. a running version of the code on your machine.
    /// </summary>
    public Handler(string name) => Name = name;

    /// <summary>
    /// The name of the intercept handler running locally.
    /// Defaults to the normalized name of this project.
    /// </summary>
    public string? Name
    {
        get => _name ??= Defaults.Name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

            if (value.Length > 64)
                throw new InvalidOperationException(Exceptions.CantExceed64Characters);

            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Exceptions.AlphaNumericWithHyphensUnderscores);

            _name = value;
        }
    }

    /// <summary>
    /// Additional environment.
    /// </summary>
    public IEnumerable<NamedValuePair<string, string>>? Environment
    {
        get => _environment;
        init
        {
            if (value == null)
                return;

            const string pattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            
            if (value.Any(environment => !Regex.IsMatch(environment.Name, pattern)))
                throw new InvalidOperationException(Exceptions.AlphaNumericWithUnderscores);

            _environment = value;
        }
    }

    /// <summary>
    /// Number of second to wait after a SIGTERM before the SIGKILL arrives.
    /// </summary>
    public int StopGracePeriod { get; init; }

    /// <summary>
    /// docker run/build/compose [OPTIONS] IMAGE[:TAG|@DIGEST] [COMMAND] [ARG...]
    /// </summary>
    public Docker? Docker { get; init; }

    /// <summary>
    /// Run a script using a shell.
    /// </summary>
    public Script? Script { get; init; }

    /// <summary>
    /// Output info to external runner.
    /// </summary>
    public External? External
    {
        get
        {
            if (_external != null)
                return _external;
            
            var temporaryDirectory = Path.Combine(Path.GetTempPath(), nameof(Telepresence).ToLowerInvariant(), Name);
            var outputPath = Path.Combine(temporaryDirectory, $"{Name}-output.json");
            
            return _external = new External
            {
                OutputFormat = OutputFormat.Json,
                OutputPath = outputPath
            };
        }
        init => _external = value;
    }
}