namespace Restaurant.Messages;

public interface IDishOrdered
{
    public Guid OrderId { get; }
        
    public Dish Dish { get; }
}

public class DishOrdered : IDishOrdered
{
    public DishOrdered(Guid orderId, Dish dish)
    {
        OrderId = orderId;
        Dish = dish;
    }

    public Guid OrderId { get; }
    public Dish Dish { get; }
}