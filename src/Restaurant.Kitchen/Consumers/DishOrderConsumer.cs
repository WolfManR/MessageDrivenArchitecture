using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class DishOrderConsumer : IConsumer<DishOrder>
{
    private readonly Chef _chef;
    private readonly ILogger<DishOrderConsumer> _logger;

    public DishOrderConsumer(Chef chef,  ILogger<DishOrderConsumer> logger)
    {
        _chef = chef;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<DishOrder> context)
    {
        var orderId = context.Message.OrderId;
        var dish = context.Message.Dish;

        if (dish is Dish.Lasagna) throw new Exception("There can not be any Lasanga");
        
        await context.Publish(new DishOrderApproved(orderId));
        
        // TODO: Might be handled on kitchen
        if(dish is null) return;
        
        // wait for dish
        await Task.Delay(2000);

        while (!_chef.CheckDishReady(orderId, dish))
        {
            await Task.Delay(300);
        }

        await context.Publish(new DishReady(orderId));
    }
}