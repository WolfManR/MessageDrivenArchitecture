using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class PreorderDishConsumer : IConsumer<IBookingRequest>
{
    private readonly Chef _chef;
    private readonly IRepository<BookingRequest> _repository;
    private readonly ILogger<PreorderDishConsumer> _logger;

    public PreorderDishConsumer(Chef chef, IRepository<BookingRequest> repository, ILogger<PreorderDishConsumer> logger)
    {
        _chef = chef;
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IBookingRequest> context)
    {
        if (context.Message.PreOrder is not {} dish) return;
        
        var orderId = context.Message.OrderId;
        var messageId = context.MessageId.ToString();

        var model = _repository.Get().FirstOrDefault(i => i.OrderId == orderId);

        if (model is not null && model.CheckMessageId(messageId))
        {
            _logger.LogInformation("BookingRequest Second time {Message}", messageId);
            return;
        }
            
        var requestModel = new BookingRequest(context.Message, messageId);

        _logger.LogInformation("BookingRequest First time {Message}", messageId);
        var resultModel = model?.Update(requestModel, messageId) ?? requestModel;
        
        _repository.AddOrUpdate(resultModel);
        
        // wait for dish
        await Task.Delay(2000);

        while (!_chef.CheckDishReady(orderId, dish))
        {
            await Task.Delay(300);
        }

        await context.Publish<IDishReady>(new DishReady(orderId));
    }
}