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

public sealed class BookingRequest : TransactionalData<BookingRequest, IBookingRequest>, IBookingRequest
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

    public BookingRequest(IBookingRequest model, string messageId) : base(model, messageId)
    {
        
    }
    
    protected override void SetData(IBookingRequest data)
    {
        OrderId = data.OrderId;
        ClientId = data.ClientId;
        PreOrder = data.PreOrder;
        CreationDate = data.CreationDate;
        IncomeTime = data.IncomeTime;
        CountOfPersons = data.CountOfPersons;
        CreationDate = data.CreationDate;
    }
    
    public Guid OrderId { get; private set;}
    public Guid ClientId { get; private set;}
    public Dish? PreOrder { get; private set;}
    
    public int IncomeTime { get; private set;}
    public int CountOfPersons { get; private set;}

    public DateTime CreationDate { get; private set;}
}