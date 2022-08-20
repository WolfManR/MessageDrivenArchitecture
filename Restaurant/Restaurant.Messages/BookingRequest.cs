namespace Restaurant.Messages;

public interface IBookingRequest
{
    Guid OrderId { get; }

    Guid ClientId { get; }

    Dish? PreOrder { get; }

    int IncomeTime { get; }
    int CountOfPersons { get; }

    DateTime CreationDate { get; }
}

public class BookingRequest : IBookingRequest
{
    public BookingRequest(Guid orderId, Guid clientId, Dish? preOrder, DateTime creationDate, int incomeTime, int countOfPersons)
    {
        OrderId = orderId;
        ClientId = clientId;
        PreOrder = preOrder;
        CreationDate = creationDate;
        IncomeTime = incomeTime;
        CountOfPersons = countOfPersons;
    }

    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public Dish? PreOrder { get; }
    
    public int IncomeTime { get; }
    public int CountOfPersons { get; }

    public DateTime CreationDate { get; }
}