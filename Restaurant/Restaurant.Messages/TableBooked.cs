namespace Restaurant.Messages;

public interface ITableBooked
{
    Guid OrderId { get; }
    bool Success { get; }
}

public class TableBooked : ITableBooked
{
    public TableBooked(Guid orderId, bool success)
    {
        OrderId = orderId;
        Success = success;
    }

    public Guid OrderId { get; }
    public bool Success { get; }
}