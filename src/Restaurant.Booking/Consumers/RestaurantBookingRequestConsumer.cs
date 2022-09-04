using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class RestaurantBookingRequestConsumer: IConsumer<BookingOrder>
{
    private readonly Manager _manager;
    private readonly ILogger<RestaurantBookingRequestConsumer> _logger;

    public RestaurantBookingRequestConsumer(
        Manager manager,
        ILogger<RestaurantBookingRequestConsumer> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingOrder> context)
    {
        var orderId = context.Message.OrderId;

        _logger.LogInformation("[OrderId: {Order}]", orderId);
        var (success, tableId) = await _manager.BookFreeTable(context.Message.CountOfPersons);
        if (!success) throw new Exception("нет свободных столов на данное число людей");
        await context.Publish(new TableBooked(orderId, tableId!.Value));
    }
}