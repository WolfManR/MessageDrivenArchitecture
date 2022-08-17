namespace Restaurant.Messages;

public interface IBookingRequest
{
    Guid OrderId { get; }

    Guid ClientId { get; }

    Dish? PreOrder { get; }

    int IncomeTime { get; }

    DateTime CreationDate { get; }
}

public class BookingRequest : IBookingRequest
{
    public BookingRequest(Guid orderId, Guid clientId, Dish? preOrder, DateTime creationDate, int incomeTime)
    {
        OrderId = orderId;
        ClientId = clientId;
        PreOrder = preOrder;
        CreationDate = creationDate;
        IncomeTime = incomeTime;
    }

    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public Dish? PreOrder { get; }
    
    public int IncomeTime { get; }

    public DateTime CreationDate { get; }
}