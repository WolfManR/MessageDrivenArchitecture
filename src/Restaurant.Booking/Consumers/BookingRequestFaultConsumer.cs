using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class BookingRequestFaultConsumer : IConsumer<Fault<BookingRequest>>
{
    private readonly ILogger<BookingRequestFaultConsumer> _logger;

    public BookingRequestFaultConsumer(ILogger<BookingRequestFaultConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<BookingRequest>> context)
    {
        _logger.LogWarning("[OrderId {Order}] Отмена в зале", context.Message.Message.OrderId);
        return Task.CompletedTask;
    }
}