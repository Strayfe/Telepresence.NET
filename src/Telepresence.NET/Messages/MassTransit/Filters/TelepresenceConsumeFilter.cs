using MassTransit;
using Telepresence.NET.Services;

namespace Telepresence.NET.Messages.MassTransit.Filters;

public class TelepresenceConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
{
    private readonly ITelepresenceApiService _telepresenceApiService;

    public TelepresenceConsumeFilter(ITelepresenceApiService telepresenceApiService) => 
        _telepresenceApiService = telepresenceApiService;

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        var interceptHeaders = new Dictionary<string, string>();

        if (context.TryGetHeader<string>(Constants.Defaults.Headers.TelepresenceInterceptAs, out var interceptAs))
            interceptHeaders.Add(Constants.Defaults.Headers.TelepresenceInterceptAs, interceptAs);

        // todo: look for any headers added to the running intercept, the developer may not want to use the default
        //       provided or may want to use more so we need to make sure the events contains all of them or else
        //       the api server is going to report consume when it shouldn't

        // even if there were no headers, we still want to check ConsumeHere because we need to always know if we should
        if (await _telepresenceApiService.ConsumeHere(interceptHeaders))
        {
            await next.Send(context);
        }

        // todo: figure out what we want to do for skipped messages, because any that are not due to be actioned will skip
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("telepresence");
}