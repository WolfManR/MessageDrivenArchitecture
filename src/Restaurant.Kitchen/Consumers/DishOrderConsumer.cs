using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class DishOrderConsumer : IConsumer<IDishOrder>
{
    private readonly Chef _chef;
    private readonly IRepository<DishOrder> _repository;
    private readonly ILogger<DishOrderConsumer> _logger;

    public DishOrderConsumer(Chef chef, IRepository<DishOrder> repository, ILogger<DishOrderConsumer> logger)
    {
        _chef = chef;
        _repository = repository;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<IDishOrder> context)
    {
        var orderId = context.Message.OrderId;
        var dish = context.Message.Dish;

        if (dish is Dish.Lasagna) throw new Exception("There can not be any Lasanga");
        
        await context.Publish<DishOrderApproved>(new DishOrderApproved() { OrderId = orderId });
        
        // TODO: Might be handled on kitchen
        if(dish is null) return;
        
        // wait for dish
        await Task.Delay(2000);

        while (!_chef.CheckDishReady(orderId, dish))
        {
            await Task.Delay(300);
        }

        await context.Publish<IDishReady>(new DishReady(orderId));
    }
}