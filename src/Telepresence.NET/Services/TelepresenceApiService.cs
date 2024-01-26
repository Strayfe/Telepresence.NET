using Newtonsoft.Json;
using Serilog;
using Telepresence.NET.Helpers;
using Telepresence.NET.Models.API;

namespace Telepresence.NET.Services;

public class TelepresenceApiService : ITelepresenceApiService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public TelepresenceApiService(IHttpClientFactory httpClientFactory)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<bool> Healthz()
    {
        if (!EnvironmentHelper.TryGetEnvironmentVariable<int>(Constants.Environment.TelepresenceApiPort, out var apiPort))
        {
            _logger.Warning("Missing environment variable: [{TelepresenceApiPort}], check your configuration", Constants.Environment.TelepresenceApiPort);
            return false;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost:{apiPort}/healthz"));
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);
        
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "An issue occurred while sending request to Telepresence RESTful API endpoint: '/healthz'");
            return false;
        }
        
        return true;
    }

/*
// todo: integration tests

a)
provided I am a service hosted anywhere
    and I cannot contact the telepresence RESTful API server
        I should consume the message?

b)
provided I am a service in the cluster
    and I have no active intercept
        I should consume the message

c)
provided I am a service in the cluster
    and I have an active intercept
    and the message was NOT sent by the developer intercepting
        I should consume the message

d)
provided I am a service in the cluster
    and I have an active intercept
    and the message was sent by the developer intercepting
        I should NOT consume the message

e)
provided I am a service on a local machine
    and I am receiving intercepted requests
    and the message was sent by a different developer
        I should NOT consume the message

f)
provided I am a service on a local machine
    and I am receiving intercepted requests
    and the message was sent by the developer debugging me
        I should consume the message

*/
    public async Task<bool> ConsumeHere(IDictionary<string, string>? interceptHeaders = null, string? optionalPath = null)
    {
        if (!EnvironmentHelper.TryGetEnvironmentVariable<int>(Constants.Environment.TelepresenceApiPort, out var apiPort))
        {
            _logger.Warning("Missing environment variable: [{TelepresenceApiPort}], check your configuration", Constants.Environment.TelepresenceApiPort);
            return true;
        }
        
        var path = $"http://localhost:{apiPort}/consume-here";
        
        if (!string.IsNullOrWhiteSpace(optionalPath))
            path = $"{path}?path={optionalPath}";

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        
        if (EnvironmentHelper.TryGetEnvironmentVariable<string>(Constants.Environment.TelepresenceInterceptId, out var interceptId))
            request.Headers.Add(Constants.Defaults.Headers.TelepresenceInterceptId, interceptId);
        
        if (interceptHeaders != null && interceptHeaders.Any())
        {
            foreach (var header in interceptHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "An issue occurred while sending request to Telepresence RESTful API endpoint: '/consume-here'");
            return true;
        }
        
        var result = await response.Content.ReadAsStringAsync();

        // result comes with a `\n` so we need to check contains...
        return result.Contains("true", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<InterceptInfo?> InterceptInfo(string? optionalPath = null)
    {
        if (!EnvironmentHelper.TryGetEnvironmentVariable<int>(Constants.Environment.TelepresenceApiPort, out var apiPort))
        {
            _logger.Warning("Missing environment variable: [{TelepresenceApiPort}], check your configuration", Constants.Environment.TelepresenceApiPort);
            return null;
        }
        
        var path = $"http://localhost:{apiPort}/intercept-info";
        
        if (!string.IsNullOrWhiteSpace(optionalPath))
            path = $"{path}?path={optionalPath}";
        
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(path);
        
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "An issue occurred while sending request to Telepresence RESTful API endpoint: '/intercept-info'");
            return null;
        }
        
        var result = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<InterceptInfo>(result);
    }
}