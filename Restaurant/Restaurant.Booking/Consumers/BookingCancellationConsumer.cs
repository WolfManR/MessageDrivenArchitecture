using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class BookingCancellationConsumer : IConsumer<IBookingCancellation>
{
    private readonly ILogger<BookingCancellationConsumer> _logger;
    private readonly Manager _manager;

    public BookingCancellationConsumer(ILogger<BookingCancellationConsumer> logger, Manager manager)
    {
        _logger = logger;
        _manager = manager;
    }
    
    public async Task Consume(ConsumeContext<IBookingCancellation> context)
    {
        if(context.Message.TableId is not {} tableId) return;
        await _manager.FreeTableAsync(tableId);
        _logger.LogInformation("[OrderId {Order}] Отмена в зале, освобождён столик {TableId}", context.Message.OrderId, tableId);
    }
}