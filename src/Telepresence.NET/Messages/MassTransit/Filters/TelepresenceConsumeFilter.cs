using MassTransit;
using Serilog;
using Telepresence.NET.Services;

namespace Telepresence.NET.Messages.MassTransit.Filters;

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

public class TelepresenceConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
{
    private readonly ITelepresenceApiService _telepresenceApiService;

    public TelepresenceConsumeFilter(ITelepresenceApiService telepresenceApiService) => 
        _telepresenceApiService = telepresenceApiService;

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        var interceptHeaders = GetInterceptHeaders(context);
        
        if (await _telepresenceApiService.ConsumeHere(interceptHeaders))
        {
            await next.Send(context);
        }
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("telepresence");

    private static IDictionary<string, string> GetInterceptHeaders(ConsumeContext<TMessage> context)
    {
        var interceptHeaders = new Dictionary<string, string>();
        
        if (context.TryGetHeader<string>(Constants.Defaults.Headers.TelepresenceInterceptAs, out var interceptAs))
            interceptHeaders.Add(Constants.Defaults.Headers.TelepresenceInterceptAs, interceptAs);

        // todo: work out a way to get any non-default headers set by custom intercept specifications
        // i.e. `--http-headers=a=b,b=c,x=y`
        
        return interceptHeaders;
    }
}