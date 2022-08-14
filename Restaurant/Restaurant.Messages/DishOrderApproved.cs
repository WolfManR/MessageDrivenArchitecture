namespace Restaurant.Messages;

public interface IDishOrderApproved
{
    Guid OrderId { get; }
    bool Approved { get; }
}

public class DishOrderApproved : IDishOrderApproved
{
    public DishOrderApproved(Guid orderId, bool approved)
    {
        OrderId = orderId;
        Approved = approved;
    }

    public Guid OrderId { get; }
    public bool Approved { get; }
}