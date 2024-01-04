using System.Text.RegularExpressions;
using Telepresence.NET.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Telepresence.NET.Models.Intercept;

/// <summary>
/// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
/// </summary>
public class InterceptSpecification
{
    /// <summary>
    /// An intercept specification and processes to control intercepting workloads in a kubernetes cluster.
    /// </summary>
    public InterceptSpecification(string name)
    {
        Name = name;
    }
    
    private string? _name;
    // todo: private IEnumerable<Prerequisites> _prerequisites;
    private Connection? _connection;
    private IEnumerable<Workload>? _workloads;
    private IEnumerable<Handler>? _handlers;
    
    /// <summary>
    /// A name to give to the specification.
    /// </summary>
    public string Name
    {
        get => _name ??= Constants.Defaults.NormalizedEntryAssembly;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(Name));
    
            const string pattern = "^[a-zA-Z][a-zA-Z0-9_-]*$";
            
            if (!Regex.IsMatch(value, pattern))
                throw new InvalidOperationException(Constants.Exceptions.AlphaNumericWithHyphens);
    
            _name = value;
        }
    }
    
    /// <summary>
    /// Connection properties to use when Telepresence connects to the cluster.
    /// </summary>
    public Connection? Connection
    {
        get => _connection ??= new Connection();
        init => _connection = value ?? throw new ArgumentNullException(nameof(Connection));
    }
    
    /// <summary>
    /// Remote workloads that are intercepted, keyed by workload name.
    /// </summary>
    public IEnumerable<Workload> Workloads
    {
        get => _workloads ??= new List<Workload>
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Workloads));
    
            if (!value.Any() || value.Count() > 32)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfWorkloadsDefined);
    
            _workloads = value;
        }
    }
    
    /// <summary>
    /// Local services running on the host machine that handle the intercepted services requests.
    /// </summary>
    public IEnumerable<Handler> Handlers
    {
        get => _handlers ??= new List<Handler>
        {
            new() { Name = _name }
        };
        init
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Handlers));
    
            if (!value.Any() || value.Count() > 64)
                throw new InvalidOperationException(Constants.Exceptions.InvalidNumberOfHandlersDefined);
            
            // assert that each handler has at least one handler
            foreach (var handler in value)
            {
                var isDocker = handler.Docker != null;
                var isScript = handler.Script != null;
                var isExternal = handler.External != null;
    
                var mutuallyExclusive = isDocker ^ isScript ^ isExternal;
                
                if (!mutuallyExclusive)
                    throw new InvalidOperationException(Constants.Exceptions.MutuallyExclusiveHandlers);
            }
            
            _handlers = value;
        }
    }
    
    /// <summary>
    /// Parse the intercept specification as YAML.
    /// Falls back to the name of the object in event of failure.
    /// </summary>
    public override string? ToString()
    {
        var result = base.ToString();
    
        try
        {
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
    
            result = serializer.Serialize(this);
        }
        catch (Exception)
        {
            // ignored
        }
    
        return result;
    }
}