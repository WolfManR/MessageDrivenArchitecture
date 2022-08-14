using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Notification.Consumers;

public class DishReadyConsumer : IConsumer<IDishReady>
{
    private readonly Notifier _notifier;

    public DishReadyConsumer(Notifier notifier)
    {
        _notifier = notifier;
    }
    
    public Task Consume(ConsumeContext<IDishReady> context)
    {
        _notifier.Accept(context.Message.OrderId, Accepted.Kitchen);
        return Task.CompletedTask;
    }
}