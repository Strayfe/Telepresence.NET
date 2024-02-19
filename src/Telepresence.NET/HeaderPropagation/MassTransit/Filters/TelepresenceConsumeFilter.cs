using MassTransit;
using Serilog;
using Telepresence.NET.RestfulApi;

namespace Telepresence.NET.HeaderPropagation.MassTransit.Filters;

/// <summary>
/// Fetch intercept headers and metadata to decide if a message should be consumed here.
/// </summary>
/// <remarks>
/// Only headers that contain "x-telepresence" will be propagated, otherwise we run the risk of propagating headers used
/// by the MassTransit internal transport mechanisms.
/// </remarks>
public class TelepresenceConsumeFilter<TMessage>(
    TelepresenceContext telepresenceContext,
    ITelepresenceApiService telepresenceApiService)
    : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        var eventHeaders = context
            .Headers
            .Where(x => x.Key.Contains("x-telepresence"));

        foreach (var header in eventHeaders)
            telepresenceContext.InterceptHeaders.TryAdd(header.Key, header.Value.ToString()!);

        if (await telepresenceApiService.ConsumeHere())
            await next.Send(context);
    }

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope("telepresence");
}
