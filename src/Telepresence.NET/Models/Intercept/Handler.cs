using System.Reflection;
using System.Text.RegularExpressions;
using Telepresence.NET.Models.Intercept.Handlers;

namespace Telepresence.NET.Models.Intercept;

public class Handler
{
    private readonly string? _name;
    private readonly IEnumerable<NamedValuePair<string, string>>? _environment;

    public Handler()
    {
        _name = Assembly
            .GetEntryAssembly()?
            .GetName()
            .Name?
            .Replace('.', '-')
            .Replace('_', '-')
            .ToLowerInvariant();
    }

    public Handler(string name) => Name = name;

    /// <summary>
    /// The name of the intercept handler running locally.
    /// Defaults to the normalized name of this project.
    /// </summary>
    public string? Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new ArgumentNullException(nameof(Name));

            return _name;
        }
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);

            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphensUnderscores);

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
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithUnderscores);

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
    public External? External { get; init; }
}