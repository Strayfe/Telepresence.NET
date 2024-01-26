# Telepresence.NET: Telepresence CLI and RESTful API Wrapper

This package serves as a convenient wrapper around the 
[Telepresence CLI](https://www.getambassador.io/docs/telepresence-oss/latest/reference/client) and its corresponding 
RESTful API.

## About
Managing the lifecycle of connecting and intercepting workloads via the Telepresence CLI and injecting the environment
variables from the cluster into the running debug instance can be quite a troubling task.

This package aims to simplify that process by allowing you to specify parameters during the launch of your application 
which will handle connecting to the cluster, starting an intercept, and automatically injecting environment variables
into the application.

## Features

- Leverages Telepresence's capabilites for connecting to and controlling different services in a development cluster.
- Abstracts the complexity of dealing with CLI commands and APIs, giving you a clean implementation to work with.
- Handles lifecycle management of connections, ensuring smooth and consistent operations.
- Facilitates productive debugging and development workflow.
- Fully control and automate injection of environment variables from the cluster or a static list into your application.

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

See the Samples for examples on how to use it. 
I will write more about the setup process here when I work on adding back in the features for Intercept Specifications.
