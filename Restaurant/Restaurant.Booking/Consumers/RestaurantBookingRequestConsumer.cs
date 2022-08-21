﻿using MassTransit;
using Restaurant.Booking.Data;
using Restaurant.Messages;

namespace Restaurant.Booking.Consumers;

public class RestaurantBookingRequestConsumer: IConsumer<IBookingRequest>
{
    private readonly Manager _manager;
    private readonly IRepository<BookingRequestModel> _repository;
    private readonly ILogger<RestaurantBookingRequestConsumer> _logger;

    public RestaurantBookingRequestConsumer(
        Manager manager,
        IRepository<BookingRequestModel> repository,
        ILogger<RestaurantBookingRequestConsumer> logger)
    {
        _manager = manager;
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBookingRequest> context)
    {
        var orderId = context.Message.OrderId;
        var messageId = context.MessageId.ToString();

        var model = _repository.Get().FirstOrDefault(i => i.OrderId == orderId);

        if (model is not null && model.CheckMessageId(messageId))
        {
            _logger.LogInformation("Second time {Message}", messageId);
            return;
        }
            
        var requestModel = new BookingRequestModel(
            orderId,
            context.Message.ClientId,
            context.Message.PreOrder,
            context.Message.CreationDate,
            messageId);

        _logger.LogInformation("First time {Message}", messageId);
        var resultModel = model?.Update(requestModel, messageId) ?? requestModel;
        
        _repository.AddOrUpdate(resultModel);

        _logger.LogInformation("[OrderId: {Order}]", orderId);
        var (success, tableId) = await _manager.BookFreeTable(context.Message.CountOfPersons);
        if (!success) throw new Exception("нет свободных столов на данное число людей");
        await context.Publish<ITableBooked>(new TableBooked(orderId, tableId!.Value));
    }
}