namespace Restaurant.Messages;

public interface IDishOrder
{
    public Guid OrderId { get; }
        
    public Dish? Dish { get; }
}

public class DishOrder : IDishOrder
{
    public DishOrder(Guid orderId, Dish? dish)
    {
        OrderId = orderId;
        Dish = dish;
    }

    public Guid OrderId { get; }
    public Dish? Dish { get; }
}