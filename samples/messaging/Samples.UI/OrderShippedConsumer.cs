using MassTransit;
using MassTransit.Configuration;
using MassTransit.Metadata;
using Samples.Contracts;

namespace Samples.UI;

public class OrderShippedConsumer(ILogger<OrderShippedConsumer> logger)
    : IConsumer<OrderShipped>
{
    public Task Consume(ConsumeContext<OrderShipped> context)
    {
        logger.LogInformation("Received Order Shipped Confirmation: {Id}", context.Message.Id);

        return Task.CompletedTask;
    }
}

public sealed class OrderShippedConsumerDefinition : ConsumerDefinition<OrderShippedConsumer>
{
    private readonly IHostEnvironment _environment;

    public OrderShippedConsumerDefinition(IHostEnvironment environment)
    {
        _environment = environment;

        // if running locally, we can make the exchanges/queues delete themselves on-close
#if DEBUG
        if (_environment.IsDevelopment() && !HostMetadataCache.IsRunningInContainer)
            EndpointDefinition = new ConsumerEndpointDefinition<OrderShippedConsumer>(
                new EndpointSettings<IEndpointDefinition<OrderShippedConsumer>>
                {
                    IsTemporary = true
                });
#endif
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<OrderShippedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // todo: consider maybe throwing a specific exception and handling it as a fault with a filter so we don't have
        //       to hide genuine dead-letters
        if (_environment.IsDevelopment())
            endpointConfigurator.DiscardSkippedMessages();
    }
}
