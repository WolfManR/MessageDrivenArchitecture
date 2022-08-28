using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class BookingCancellationConsumer : IConsumer<IBookingCancellation>
{
    private readonly ILogger<BookingCancellationConsumer> _logger;
    private readonly Manager _manager;
    private readonly IRepository<BookingCancellation> _repository;

    public BookingCancellationConsumer(
        ILogger<BookingCancellationConsumer> logger,
        Manager manager,
        IRepository<BookingCancellation> repository)
    {
        _logger = logger;
        _manager = manager;
        _repository = repository;
    }
    
    public async Task Consume(ConsumeContext<IBookingCancellation> context)
    {
        if(context.Message.TableId is not {} tableId) return;
        
        var orderId = context.Message.OrderId;
        var messageId = context.MessageId.ToString();

        var model = _repository.Get().FirstOrDefault(i => i.OrderId == orderId);

        if (model is not null && model.CheckMessageId(messageId))
        {
            _logger.LogDebug("Cancellation Second time {Message}", messageId);
            return;
        }
            
        var requestModel = new BookingCancellation(context.Message, messageId);

        _logger.LogDebug("Cancellation First time {Message}", messageId);
        var resultModel = model?.Update(requestModel, messageId) ?? requestModel;
        
        _repository.AddOrUpdate(resultModel);
        
        await _manager.FreeTableAsync(tableId);
        _logger.LogInformation("[OrderId {Order}] Отмена в зале, освобождён столик {TableId}", context.Message.OrderId, tableId);
    }
}