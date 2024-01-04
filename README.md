# Telepresence.NET: Telepresence CLI and RESTful API Wrapper

This package serves as a convenient wrapper around the 
[Telepresence CLI](https://www.getambassador.io/docs/telepresence-oss/latest/reference/client) and its corresponding 
RESTful API.

## About
Managing the lifecycle of connecting and intercepting services in a cluster can become a complex task.
This package aims to simplify that process by allowing users to control these aspects directly through their solution.
This results in no longer needing to manually handle each step, increasing productivity and efficiency.

## Features

- Leverages Telepresence's capabilites for connecting to and controlling different services in a development cluster.
- Abstracts the complexity of dealing with CLI commands and APIs, giving you a clean implementation to work with.
- Handles lifecycle management of connections, ensuring smooth and consistent operations.
- Facilitates productive debugging and development workflow.
- 

## Limitations

As this product is still in it's infancy, there are some limitations to it's usage and it is currently slightly
opinionated to the way we operate in my workplace, however, I will be working to abstract the product as much as possible.

- only supports premium tiers due to using intercept specs to build up the connection, an oversight from my subscription, will address soon
- Opinionated conventions
- Only works with the first element of collections of handlers, workloads and intercepts
- Connection does not have an implicit or default, this will be coming soon and will be determined automatically from \
  the users kubeconfig
- Prerequisites have not yet been coded in
- Docker handlers have not yet been coded in
- Due to limitations with how certain IDEs do not send interrupt signals to gracefully shutdown applications during \
  debugging, cleanup operations cannot always be run automatically at the stopping of the application
- 

## Installation

To start using this package, install it through NuGet package manager or CLI as shown below:

Through your NuGet package manager:
- Search for "Telepresence.NET" and click on install.

or

Through .NET CLI:
- Run the following command:
    ```
    dotnet add package Telepresence.NET 
    ```

## Usage

### Implicit / Convention Based

The CLI and therefore this wrapper have implicit ways to connect to a cluster and some assumptions have been made
about the structure of workload architecture to support an easy installation.

In your program before anything starts up, you want to create a new intercept object.
This will set up a new intercept specification with some assumed defaults.

```csharp
using Telepresence.NET;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// set up intercept specification
var intercept = new Intercept();

// start the intercept
await intercept.Start();
#endif

// optional - add environment variables from the cluster to your process
builder.Configuration.AddEnvironmentVariables();

// ... snipped for brevity
```

### Explicit

Changes to the defaults can easily be made through object initialization.
The below example demonstrates creating objects with lists or arrays, any type of enumerable should work.

```csharp
var intercept = new Intercept
{
    Connection = new Connection
    {
        Context = "my_cluster",
        Namespace = "my_namespace"
    },
    Handlers = new List<Handler>
    {
        new Handler
        {
            Name = "my_handler",
            External = new External
            {
                OutputFormat = OutputFormat.Json,
                OutputPath = "."
            }
        }
    },
    Workloads = new Workload[]
    {
        new Workload
        {
            Name = "my_workload",
            Intercepts = new WorkloadIntercept[]
            {
                new WorkloadIntercept
                {
                    Name = "my_intercept",
                    
                }
            }
        }
    }
};
```

### Forwarding Downstream Requests (for Personal Intercepts)

Working on multiple services concurrently can be quite frustrating when requests are not communicating between multiple
intercepts, to combat this we can leverage the `TelepresenceDelegatingHandler` to set up a `HttpClient` to automatically
forward requests onto intercepted services.

Included in the package is a `TelepresenceDelegatingHandler` that you can reference directly or if you are setting up
a Dependency Injection Container then you can use the extensions as follows.

```csharp
using Telepresence.NET;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// set up intercept specification
var intercept = new Intercept();

// start the intercept
await intercept.Start();
#endif

// optional - add environment variables from the cluster to your process
builder.Configuration.AddEnvironmentVariables();

// register telepresence DI services and request forwarding
builder.Services
    .AddTelepresence()
    .WithRequestForwarding();

// add named HttpClient with request forwarding
builder.Services.AddHttpClient("my_client")
    .AddHttpMessageHandler<TelepresenceDelegatingHandler>();

// add typed HttpClient with request forwarding
builder.Services.AddHttpClient<ExampleService>()
    .AddHttpMessageHandler<TelepresenceDelegatingHandler>();

// ... snipped for brevity
```

## Troubleshooting

If something isn't quite right, you can see the output of the intercept specification in it's raw format.
Simply call `.ToString()` on the intercept object and it should return it's processed YAML format.

```csharp
var intercept = new Intercept();
var spec = intercept.ToString();
Console.WriteLine(spec);

/* output:

name: example
workloads:
- name: example
  intercepts:
  - enabled: true
    handler: example
    name: example
    localPort: 65090
    service: example
    headers:
    - name: x-telepresence-intercept-as
      value: '{{ .Telepresence.Username }}'
handlers:
- name: example
  external:
    isDocker: false
    outputFormat: json
    outputPath: /tmp/telepresence/example/example-output.json

*/
```

