using Telepresence.NET;
using Telepresence.NET.Models;
using Telepresence.NET.Models.Intercept;

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

await intercept.Start();

// demonstrate that the environment variables were loaded
Console.WriteLine(Environment.GetEnvironmentVariable("TEST_ENVIRONMENT_VARIABLE"));

Console.WriteLine("Waiting for user input...");

Console.ReadLine();

await intercept.Leave();
