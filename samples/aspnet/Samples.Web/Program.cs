using Microsoft.AspNetCore.Mvc;
using Telepresence.NET;
using Telepresence.NET.Connection;
using Telepresence.NET.Extensions;
using Telepresence.NET.Intercept;
using Telepresence.NET.Services;

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
    }
};

// start the intercept
await intercept.Start();

// run your application
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddTelepresence()
        .WithRequestForwarding()
        .WithRestfulApi();

    var app = builder.Build();

    app.MapGet("/", async ([FromServices] ITelepresenceApiService apiService) =>
    {
        var interceptHeaders = new Dictionary<string, string>
        {
            {
                Constants.Defaults.Headers.TelepresenceInterceptAs,
                Environment.UserName
            }
        };

        var healthy = await apiService.Healthz();
        var consumeHere = await apiService.ConsumeHere(interceptHeaders);
        var interceptInfo = await apiService.InterceptInfo();

        var response = new
        {
            healthy,
            consumeHere,
            interceptInfo
        };
        
        return response;
    });

    app.Run();
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