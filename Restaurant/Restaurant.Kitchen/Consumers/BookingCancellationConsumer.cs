﻿using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class BookingCancellationConsumer : IConsumer<IBookingCancellation>
{
    private readonly ILogger<BookingCancellationConsumer> _logger;

    public BookingCancellationConsumer(ILogger<BookingCancellationConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<IBookingCancellation> context)
    {
        _logger.LogInformation("[OrderId {Order}] Отмена готовки блюда", context.Message.OrderId);
        return context.ConsumeCompleted;
    }
}