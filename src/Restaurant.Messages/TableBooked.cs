namespace Restaurant.Messages;

public interface ITableBooked
{
    Guid OrderId { get; }
    int TableId { get; }
}

public class TableBooked : ITableBooked
{
    public TableBooked(Guid orderId, int tableId)
    {
        OrderId = orderId;
        TableId = tableId;
    }

    public Guid OrderId { get; }
    public int TableId { get; }
}