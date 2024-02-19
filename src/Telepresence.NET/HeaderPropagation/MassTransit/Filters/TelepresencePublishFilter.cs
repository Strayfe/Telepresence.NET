using MassTransit;

namespace Telepresence.NET.HeaderPropagation.MassTransit.Filters;

/// <summary>
/// Propagate intercept headers and metadata onto downstream events.
/// </summary>
public class TelepresencePublishFilter<T>(TelepresenceContext telepresenceContext)
    : IFilter<PublishContext<T>>
    where T : class
{
    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        foreach (var header in telepresenceContext.InterceptHeaders)
            context.Headers.Set(header.Key, header.Value);

        return next.Send(context);
    }

    public void Probe(ProbeContext context) =>
       context.CreateFilterScope("telepresence");
}
