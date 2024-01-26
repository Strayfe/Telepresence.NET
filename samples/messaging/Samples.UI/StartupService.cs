using MassTransit;
using Samples.Contracts;

namespace Samples.UI;

public class StartupService(IBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // demonstrate the UI generating orders
        while (true)
        {
            await bus.Publish(new OrderCreated
            {
                Id = Guid.NewGuid(),
            }, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}