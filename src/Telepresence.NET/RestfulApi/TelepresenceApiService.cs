using Newtonsoft.Json;
using Serilog;
using Telepresence.NET.HeaderPropagation;
using Telepresence.NET.Helpers;
using Telepresence.NET.RestfulApi.Models;

namespace Telepresence.NET.RestfulApi;

public class TelepresenceApiService(
    TelepresenceContext telepresenceContext,
    IHttpClientFactory httpClientFactory)
    : ITelepresenceApiService
{
    private readonly ILogger _logger = new LoggerConfiguration()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][telepresence] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    public async Task<bool> Healthz()
    {
        if (!EnvironmentHelper.TryGetEnvironmentVariable<int>(Constants.Environment.TelepresenceApiPort, out var apiPort))
        {
            _logger.Warning("Missing environment variable: [{TelepresenceApiPort}], check your configuration", Constants.Environment.TelepresenceApiPort);
            return false;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost:{apiPort}/healthz"));
        var httpClient = httpClientFactory.CreateClient();
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

    public async Task<bool> ConsumeHere(string? optionalPath = null)
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

        foreach (var header in telepresenceContext.InterceptHeaders)
            request.Headers.Add(header.Key, header.Value);

        var httpClient = httpClientFactory.CreateClient();
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

        var request = new HttpRequestMessage(HttpMethod.Get, path);

        if (EnvironmentHelper.TryGetEnvironmentVariable<string>(Constants.Environment.TelepresenceInterceptId, out var interceptId))
            request.Headers.Add(Constants.Defaults.Headers.TelepresenceInterceptId, interceptId);

        foreach (var header in telepresenceContext.InterceptHeaders)
            request.Headers.Add(header.Key, header.Value);

        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);

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