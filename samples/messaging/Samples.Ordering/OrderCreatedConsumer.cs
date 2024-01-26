using MassTransit;
using MassTransit.Configuration;
using MassTransit.Metadata;
using Samples.Contracts;

namespace Samples.Ordering;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        await context.Publish(new OrderShipped
        {
            Id = context.Message.Id
        });
    }
}

public sealed class OrderCreatedConsumerDefinition : ConsumerDefinition<OrderCreatedConsumer>
{
    private readonly IHostEnvironment _environment;

    public OrderCreatedConsumerDefinition(IHostEnvironment environment)
    {
        _environment = environment;

        // if running locally, we can make the exchanges/queues delete themselves on-close
#if DEBUG
        if (_environment.IsDevelopment() && !HostMetadataCache.IsRunningInContainer)
            EndpointDefinition = new ConsumerEndpointDefinition<OrderCreatedConsumer>(
                new EndpointSettings<IEndpointDefinition<OrderCreatedConsumer>>
                {
                    IsTemporary = true
                });
#endif
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<OrderCreatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // todo: consider maybe throwing a specific exception and handling it as a fault with a filter so we don't have
        //       to hide genuine dead-letters
        if (_environment.IsDevelopment())
            endpointConfigurator.DiscardSkippedMessages();
    }
}
