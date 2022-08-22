using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Notification.Consumers;

public class NotifyConsumer : IConsumer<INotify>
{
    private readonly Notifier _notifier;
    private readonly IRepository<Notify> _repository;
    private readonly ILogger<NotifyConsumer> _logger;

    public NotifyConsumer(
        Notifier notifier,
        IRepository<Notify> repository,
        ILogger<NotifyConsumer> logger)
    {
        _notifier = notifier;
        _repository = repository;
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<INotify> context)
    {
        var orderId = context.Message.OrderId;
        var messageId = context.MessageId.ToString();

        var model = _repository.Get().FirstOrDefault(i => i.OrderId == orderId);

        if (model is not null && model.CheckMessageId(messageId))
        {
            _logger.LogInformation("Notification Second time {Message}", messageId);
            return context.ConsumeCompleted;
        }
            
        var requestModel = new Notify(context.Message, messageId);

        _logger.LogInformation("Notification First time {Message}", messageId);
        var resultModel = model?.Update(requestModel, messageId) ?? requestModel;
        
        _repository.AddOrUpdate(resultModel);
        
        _notifier.Notify(context.Message.OrderId, context.Message.ClientId, context.Message.Message);
        return context.ConsumeCompleted;
    }
}