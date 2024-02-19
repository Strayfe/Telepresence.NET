using System.Text.RegularExpressions;

namespace Telepresence.NET.InterceptSpec;

/// <summary>
/// An intercepted workload (Deployment, ReplicaSet or StatefulSet), keyed by name.
/// </summary>
internal class Workload
{
    private string? _name;
    private IEnumerable<WorkloadIntercept>? _intercepts;

    /// <summary>
    /// An intercepted workload (Deployment, ReplicaSet or StatefulSet), keyed by name.
    /// </summary>
    public Workload()
    {
    }

    /// <summary>
    /// An intercepted workload (Deployment, ReplicaSet or StatefulSet), keyed by name.
    /// </summary>
    public Workload(string name) => Name = name;

    /// <summary>
    /// The name of the workload.
    /// Defaults to the normalized assembly name.
    /// </summary>
    public string Name
    {
        get => _name ??= Constants.Defaults.NormalizedEntryAssembly;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));

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
        get => _intercepts ??= new List<WorkloadIntercept>
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
            {
                _intercepts = value;
                return;
            }

            if (!value.Any() || value.Count() > 16)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfInterceptsDefined);

            _intercepts = value;
        }
    }
}