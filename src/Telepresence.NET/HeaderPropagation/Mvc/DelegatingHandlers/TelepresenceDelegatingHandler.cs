using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Telepresence.NET.HeaderPropagation.Mvc.DelegatingHandlers;

/// <summary>
/// A delegating handler that will automatically handle propagating intercepted HTTP headers onto downstream requests.
/// </summary>
public class TelepresenceDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var telepresenceContext = httpContextAccessor
            .HttpContext?
            .RequestServices
            .GetService<TelepresenceContext>();

        if (telepresenceContext == null || !telepresenceContext.InterceptHeaders.Any())
            return base.SendAsync(request, cancellationToken);

        foreach (var header in telepresenceContext.InterceptHeaders)
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return base.SendAsync(request, cancellationToken);
    }
}
