using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen;

public class Chef
{
    private readonly IBus _bus;

    public Chef(IBus bus)
    {
        _bus = bus;
    }
    
    public void CheckDishReady(Guid orderId, Dish? dish)
    {
        _bus.Publish<IDishReady>(new DishReady(orderId, true));
    }
}