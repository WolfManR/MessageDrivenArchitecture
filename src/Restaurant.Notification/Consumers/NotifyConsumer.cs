using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Notification.Consumers;

public class NotifyConsumer : IConsumer<Notify>
{
    private readonly Notifier _notifier;
    private readonly ILogger<NotifyConsumer> _logger;

    public NotifyConsumer(
        Notifier notifier,
        ILogger<NotifyConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<Notify> context)
    {
        _notifier.Notify(context.Message.OrderId, context.Message.ClientId, context.Message.Message);
        return context.ConsumeCompleted;
    }
}