using Telepresence.NET;
using Telepresence.NET.Connection;
using Telepresence.NET.Intercept;

// create a connection to the cluster
var connection = new Connection("debug")
{
    Context = "minikube",
    Namespace = "emojivoto",
};

// run the connection
await connection.Connect();

// create an intercept
var intercept = new Intercept("web")
{
    Use = "debug",
    Workload = "web",
    Service = "web-svc",
    HttpHeader = new[]
    {
        new KeyValuePair<string, string>
        (
            Constants.Defaults.Headers.TelepresenceInterceptAs,
            Environment.UserName
        )
    },
    EnvJson = "env.json",
    InjectEnvironment = true,
    Port = "6000",
    IncludeEnvironment = new Dictionary<string, string>
    {
        { "DOTNET_URLS", "http://+:6000" },
        { "ASPNETCORE_URLS", "http://+:6000" },
    },
    ExcludeEnvironment = new[]
    {
        "ASPNETCORE_URLS"
    }
};

// start the intercept
await intercept.Start();

// run your application

// should return http://+:6000
Console.WriteLine(Environment.GetEnvironmentVariable("DOTNET_URLS"));

// should return null/default/throw exception
Console.WriteLine(Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

Console.WriteLine("Waiting for user input...");
Console.ReadLine();