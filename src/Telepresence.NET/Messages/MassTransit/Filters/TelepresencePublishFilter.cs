using MassTransit;
using Microsoft.AspNetCore.Http;

namespace Telepresence.NET.Messages.MassTransit.Filters;

public class TelepresencePublishFilter<T> : IFilter<PublishContext<T>> where T : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TelepresencePublishFilter(IHttpContextAccessor httpContextAccessor) => 
        _httpContextAccessor = httpContextAccessor;

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        if (_httpContextAccessor.HttpContext is null)
            return next.Send(context);
        
        // todo: account for non-default headers
        if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(Constants.Defaults.Headers.TelepresenceInterceptAs, out var interceptAs))
            context.Headers.Set(Constants.Defaults.Headers.TelepresenceInterceptAs, interceptAs);

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
       context.CreateFilterScope("telepresence");
}