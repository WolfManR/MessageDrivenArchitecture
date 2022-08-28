namespace Restaurant.Messages;

public interface INotify
{
    public Guid OrderId { get; }

    public Guid ClientId { get; }

    public string Message { get; }
}

public class Notify : TransactionalData<Notify, INotify>, INotify
{
    public Notify(Guid orderId, Guid clientId, string message)
    {
        OrderId = orderId;
        ClientId = clientId;
        Message = message;
    }

    public Notify(INotify data, string messageId) : base(data, messageId)
    {
    }

    public Guid OrderId { get; private set; }
    public Guid ClientId { get; private set; }
    public string Message { get; private set; }

    protected override void SetData(INotify data)
    {
        OrderId = data.OrderId;
        ClientId = data.ClientId;
        Message = data.Message;
    }
}