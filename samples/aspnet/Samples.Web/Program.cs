using Samples.Web;
using Telepresence.NET;
using Telepresence.NET.Connection;
using Telepresence.NET.DependencyInjection;
using Telepresence.NET.HeaderPropagation.Mvc.DelegatingHandlers;
using Telepresence.NET.HeaderPropagation.Mvc.Filters;
using Telepresence.NET.Intercept;

// create a connection to the cluster
var connection = new Connection("debug")
{
    Context = "Dev_AKS",
    Namespace = "bluemountain",
};

// run the connection
await connection.Connect();

// create an intercept
var intercept = new Intercept("identity-api")
{
    Use = "debug",
    Workload = "identity-api",
    Service = "identity-api",
    HttpHeader = new[]
    {
        new KeyValuePair<string, string>
        (
            Constants.Defaults.Headers.TelepresenceInterceptAs,
            Environment.UserName
        )
    },
    InjectEnvironment = true,
    EnvJson = "env.json",
    Port = "7000",
    IncludeEnvironment = new Dictionary<string, string>
    {
        { "DOTNET_URLS", "http://+:7000" },
        { "ASPNETCORE_URLS", "http://+:7000" },
    },
    PreviewUrl = false
};

// start the intercept
await intercept.Start();

// run your application
try
{
    var builder = WebApplication.CreateBuilder(args);

    // register DI container services as required
    builder.Services
        .AddTelepresence()
        .WithHttpRequestForwarding()
        .WithRestfulApi();

    // this grabs the headers from the HttpContext for propagation
    builder.Services
        .AddControllers(x => x.Filters.Add<TelepresenceActionFilter>());

    // this is a way of creating a named http client that propagates previously captured headers
    builder.Services
        .AddHttpClient(nameof(ExampleService))
        .AddHttpMessageHandler<TelepresenceDelegatingHandler>();

    // this is a way of creating a typed http client that propagates previously captured headers
    builder.Services
        .AddHttpClient<ExampleService>()
        .AddHttpMessageHandler<TelepresenceDelegatingHandler>();

    var application = builder.Build();

    application.MapControllers();

    application.Run();
}
catch
{
    // ignored
}
finally
{
    // optionally leave the intercept
    await intercept.Leave();

    // optionally disconnect from the cluster
    await connection.Disconnect();

    // optionally quit all telepresence daemons
    await connection.Quit();
}