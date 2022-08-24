using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class BookingCancellationConsumer : IConsumer<IBookingCancellation>
{
    private readonly ILogger<BookingCancellationConsumer> _logger;
    private readonly IRepository<BookingCancellation> _repository;

    public BookingCancellationConsumer(
        ILogger<BookingCancellationConsumer> logger,
        IRepository<BookingCancellation> repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public Task Consume(ConsumeContext<IBookingCancellation> context)
    {
        var orderId = context.Message.OrderId;
        var messageId = context.MessageId.ToString();

        var model = _repository.Get().FirstOrDefault(i => i.OrderId == orderId);

        if (model is not null && model.CheckMessageId(messageId))
        {
            _logger.LogInformation("Cancellation Second time {Message}", messageId);
            return context.ConsumeCompleted;
        }
            
        var requestModel = new BookingCancellation(context.Message, messageId);

        _logger.LogInformation("Cancellation First time {Message}", messageId);
        var resultModel = model?.Update(requestModel, messageId) ?? requestModel;
        
        _repository.AddOrUpdate(resultModel);
        
        _logger.LogInformation("[OrderId {Order}] Отмена готовки блюда", context.Message.OrderId);
        return context.ConsumeCompleted;
    }
}