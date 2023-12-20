using MassTransit;
using Microsoft.AspNetCore.Http;

namespace Telepresence.NET.Messages.MassTransit.Filters;

public class TelepresenceSendFilter<T> : IFilter<SendContext<T>> where T : class
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TelepresenceSendFilter(IHttpContextAccessor httpContextAccessor) => 
        _httpContextAccessor = httpContextAccessor;

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
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