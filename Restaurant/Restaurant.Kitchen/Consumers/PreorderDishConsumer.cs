using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class PreorderDishConsumer : IConsumer<IBookingRequest>
{
    private readonly Chef _chef;

    public PreorderDishConsumer(Chef chef)
    {
        _chef = chef;
    }
    
    public async Task Consume(ConsumeContext<IBookingRequest> context)
    {
        if (context.Message.PreOrder is not {} dish) return;
        var orderId = context.Message.OrderId;
        // wait for check that dish can be cooked
        await Task.Delay(200);
        
        if (!_chef.CanCookDish(dish))
        {
            await context.Publish<IDishOrderApproved>(new DishOrderApproved(orderId, false));
            return;
        }
        
        await context.Publish<IDishOrderApproved>(new DishOrderApproved(orderId, true));
        
        // wait for dish
        await Task.Delay(2000);

        while (!_chef.CheckDishReady(orderId, dish))
        {
            await Task.Delay(300);
        }

        await context.Publish<IDishReady>(new DishReady(orderId, true));
    }
}