using System.Diagnostics;
using System.Text.RegularExpressions;
using Telepresence.NET.InterceptSpec.Handlers;
using Telepresence.NET.InterceptSpec.Models;

namespace Telepresence.NET.InterceptSpec;



/// <summary>
/// The resource where the intercepted requests will be routed to, i.e. a running version of the code on your machine.
/// </summary>
internal class Handler
{
    private string? _name;
    private readonly IEnumerable<NamedValuePair<string, string>>? _environment;
    private IHandlerStrategy? _handlerStrategy;

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

    private const string StandardOutput = "stdout";
    private const string StandardError = "stderr";

    /// <summary>
    /// The name of the intercept handler running locally.
    /// Defaults to the normalized name of this project.
    /// </summary>
    public string? Name
    {
        get => _name ??= Constants.Defaults.NormalizedEntryAssembly;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

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
    public External? External
    {
        get
        {
            if (_handlerStrategy is External external)
                return external;

            var externalHandler = new External
            {
                OutputFormat = OutputFormat.Json,
                OutputPath = StandardOutput
            };

            _handlerStrategy = externalHandler;

            return externalHandler;
        }
        init => _handlerStrategy = value;
    }

    /// <summary>
    /// Run any operations required by the handler.
    /// Sorts of things like injecting environment variables into the running process.
    /// </summary>
    public Task Handle(Process process, CancellationToken cancellationToken = default) =>
        _handlerStrategy.Handle(process, cancellationToken);
}