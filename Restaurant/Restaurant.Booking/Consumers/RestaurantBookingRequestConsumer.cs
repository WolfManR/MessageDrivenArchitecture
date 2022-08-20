using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class RestaurantBookingRequestConsumer: IConsumer<IBookingRequest>
{
    private readonly Manager _manager;
    private readonly ILogger<RestaurantBookingRequestConsumer> _logger;

    public RestaurantBookingRequestConsumer(Manager manager, ILogger<RestaurantBookingRequestConsumer> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBookingRequest> context)
    {
        _logger.LogInformation("[OrderId: {Order}]", context.Message.OrderId);
        var (success, tableId) = await _manager.BookFreeTable(context.Message.CountOfPersons);
        if (!success) throw new Exception("нет свободных столов на данное число людей");
        await context.Publish<ITableBooked>(new TableBooked(context.Message.OrderId, tableId!.Value));
    }
}