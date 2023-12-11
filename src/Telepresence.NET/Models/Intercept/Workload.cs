using System.Reflection;
using System.Text.RegularExpressions;

namespace Telepresence.NET.Models.Intercept;

/// <summary>
/// An intercepted workload (Deployment, ReplicaSet or StatefulSet), keyed by name.
/// </summary>
public class Workload
{
    private readonly string? _name;
    private readonly IEnumerable<WorkloadIntercept>? _intercepts;

    public Workload()
    {
        _name = Assembly
            .GetEntryAssembly()?
            .GetName()
            .Name?
            .Replace('.', '-')
            .Replace('_', '-')
            .ToLowerInvariant();
    }

    public Workload(string name) => Name = name;

    /// <summary>
    /// The name of the workload.
    /// Defaults to the normalized name of the current project.
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
            {
                value = Assembly
                    .GetEntryAssembly()?
                    .GetName()
                    .Name?
                    .Replace('.', '-')
                    .Replace('_', '-')
                    .ToLowerInvariant() ?? 
                        throw new InvalidOperationException(Constants.Exceptions.CantDetermineName);
            }

            if (value.Length > 64)
                throw new InvalidOperationException(Constants.Exceptions.CantExceed64Characters);
            
            const string pattern = "^[a-z][a-z0-9-]*$";

            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);

            _name = value;
        }
    }

    /// <summary>
    /// The services and/or ports to intercept.
    /// </summary>
    public IEnumerable<WorkloadIntercept>? Intercepts
    {
        get => _intercepts;
        init
        {
            if (value == null)
                return;

            if (!value.Any() || value.Count() > 16)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfInterceptsDefined);

            _intercepts = value;
        }
    }
}