using MassTransit;

namespace Telepresence.NET.HeaderPropagation.MassTransit.Filters;

/// <summary>
/// Propagate intercept headers and metadata onto downstream events.
/// </summary>
public class TelepresenceSendFilter<T>(TelepresenceContext telepresenceContext)
    : IFilter<SendContext<T>>
    where T : class
{
    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        foreach (var header in telepresenceContext.InterceptHeaders)
            context.Headers.Set(header.Key, header.Value);

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
       context.CreateFilterScope("telepresence");
}
