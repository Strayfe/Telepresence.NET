using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telepresence.NET.Options;

namespace Telepresence.NET.DelegatingHandlers;

public class TelepresenceDelegatingHandler : DelegatingHandler
{
    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DelegatingHandlerOptions _options;

    public TelepresenceDelegatingHandler(
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor, 
        IOptions<DelegatingHandlerOptions> options)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_environment.IsProduction())
            return base.SendAsync(request, cancellationToken);

        if (_httpContextAccessor.HttpContext is null)
            return base.SendAsync(request, cancellationToken);
        
        // todo: figure out how to identify if the caller is an event consumer so we can propagate headers from messages
        //       onto HTTP requests, is there an equivalent context accessor for masstransit?
        
        foreach (var interceptHeaderName in _options.InterceptHeaderNames)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(interceptHeaderName, out var headerValue))
                request.Headers.TryAddWithoutValidation(interceptHeaderName, headerValue.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}