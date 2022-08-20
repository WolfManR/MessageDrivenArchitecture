namespace Restaurant.Messages;

public interface IBookingCancellation
{
    Guid OrderId { get; }
    int? TableId { get; }
}

public class BookingCancellation : IBookingCancellation
{
    public BookingCancellation(Guid orderId, int? tableId)
    {
        OrderId = orderId;
        TableId = tableId;
    }

    public Guid OrderId { get; }
    public int? TableId { get; }
}