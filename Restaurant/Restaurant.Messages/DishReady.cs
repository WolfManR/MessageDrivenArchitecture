namespace Restaurant.Messages;

public interface IDishReady
{
    public Guid OrderId { get; }
        
    public bool Ready { get; }
}

public class DishReady : IDishReady
{
    public DishReady(Guid orderId, bool ready)
    {
        OrderId = orderId;
        Ready = ready;
    }

    public Guid OrderId { get; }
    public bool Ready { get; }
}