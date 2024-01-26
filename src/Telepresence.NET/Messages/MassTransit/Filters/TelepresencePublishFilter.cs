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
        var interceptAs = string.Empty;

        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(Constants.Defaults.Headers.TelepresenceInterceptAs, out var httpInterceptAs) &&
            !string.IsNullOrWhiteSpace(httpInterceptAs))
            interceptAs = httpInterceptAs.ToString();

        if (context.Headers.TryGetHeader(Constants.Defaults.Headers.TelepresenceInterceptAs, out var eventInterceptAs) &&
            !string.IsNullOrWhiteSpace(eventInterceptAs.ToString()))
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