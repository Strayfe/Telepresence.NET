using Telepresence.NET;
using Telepresence.NET.Extensions;
using Telepresence.NET.Models;
using Telepresence.NET.Models.Intercept;
using Telepresence.NET.Services;

var intercept = new Intercept
{
    Name = "web",
    Connection = new Connection
    {
        Context = "minikube",
        Namespace = "emojivoto"
    },
    Workloads = new List<Workload>
    {
        new()
        {
            Name = "web",
            Intercepts = new List<WorkloadIntercept>
            {
                new("web-svc")
            }
        }
    },
    Handlers = new List<Handler>
    {
        new("web-svc")
        {
            Environment = new List<NamedValuePair<string, string>>
            {
                new()
                {
                    Name = "TEST_ENVIRONMENT_VARIABLE",
                    Value = "Hello World"
                }
            }
        }
    }
};

Console.WriteLine(intercept.ToString());

// needs to be run before the web application is built if environment variables need to be loaded from the cluster
await intercept.Start();

// demonstrate that the environment variables were loaded
Console.WriteLine(Environment.GetEnvironmentVariable("TEST_ENVIRONMENT_VARIABLE"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelepresence()
    .WithRequestForwarding()
    .WithRestfulApi();

var app = builder.Build();

app.MapGet("/", async (ITelepresenceApiService apiService) =>
{
    var interceptHeaders = new Dictionary<string, string>
    {
        {
            Constants.Defaults.Headers.TelepresenceInterceptAs,
            "Strayfe"
        }
    };
    
    // var healthy = await apiService.Healthz();
    var consumeHere = await apiService.ConsumeHere(interceptHeaders);
    // var interceptInfo = await apiService.InterceptInfo();
    
    return $"consumeHere: {consumeHere}";
});

app.Run();
