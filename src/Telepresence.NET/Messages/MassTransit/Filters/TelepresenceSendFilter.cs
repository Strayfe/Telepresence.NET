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
        var interceptAs = string.Empty;

        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(Constants.Defaults.Headers.TelepresenceInterceptAs, out var httpInterceptAs))
            interceptAs = httpInterceptAs.ToString();

        if (context.Headers.TryGetHeader(Constants.Defaults.Headers.TelepresenceInterceptAs, out var eventInterceptAs))
            interceptAs = eventInterceptAs.ToString();

        if (!string.IsNullOrWhiteSpace(interceptAs))
            context.Headers.Set(Constants.Defaults.Headers.TelepresenceInterceptAs, interceptAs);

        // todo: look for any headers added to the running intercept, the developer may not want to use the default
        //       provided or may want to use more so we need to make sure the events contains all of them or else
        //       the api server is going to report consume when it shouldn't

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
       context.CreateFilterScope("telepresence");
}