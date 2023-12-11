using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telepresence.NET.Options;

namespace Telepresence.NET.DelegatingHandlers;

public class TelepresenceDelegatingHandler : DelegatingHandler
{
    private const string HeaderName = "x-telepresence-intercept-as";

    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly string _interceptHeaderName;

    public TelepresenceDelegatingHandler(
        IHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor, 
        IOptions<DelegatingHandlerOptions> options)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _interceptHeaderName = options.Value.InterceptHeaderName ?? HeaderName;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return base.SendAsync(request, cancellationToken);

        // todo: figure out how to identify if the caller is an event consumer so we can force adding in the header
        if (_httpContextAccessor.HttpContext is null)
            return base.SendAsync(request, cancellationToken);

        if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_interceptHeaderName, out var routeAs))
            request.Headers.TryAddWithoutValidation(_interceptHeaderName, routeAs.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}