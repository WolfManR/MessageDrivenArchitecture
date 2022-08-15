namespace Restaurant.Messages;

public interface IDishReady
{
    public Guid OrderId { get; }
}

public class DishReady : IDishReady
{
    public DishReady(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}