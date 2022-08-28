namespace Restaurant.Messages;

public interface IBookingCancellation
{
    Guid OrderId { get; }
    int? TableId { get; }
}

public class BookingCancellation : TransactionalData<BookingCancellation, IBookingCancellation>, IBookingCancellation
{
    public BookingCancellation(Guid orderId, int? tableId)
    {
        OrderId = orderId;
        TableId = tableId;
    }

    public BookingCancellation(IBookingCancellation data, string messageId) : base(data, messageId)
    {
    }

    protected override void SetData(IBookingCancellation data)
    {
        OrderId = data.OrderId;
        TableId = data.TableId;
    }

    public Guid OrderId { get; private set; }
    public int? TableId { get; private set; }
}