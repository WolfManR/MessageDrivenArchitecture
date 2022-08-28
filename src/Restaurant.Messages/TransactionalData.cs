namespace Restaurant.Messages;

public abstract class TransactionalData<TSelf, TData> where TSelf : TransactionalData<TSelf, TData>
{
    private readonly List<string> _messageIds = new();

    protected TransactionalData() { }

    protected TransactionalData(TData data, string messageId)
    {
        _messageIds.Add(messageId);
        
        SetData(data);
    }
    
    public TSelf Update(TData model, string messageId)
    {
        _messageIds.Add(messageId);
        
        SetData(model);
        return (TSelf)this;
    }
    
    public bool CheckMessageId(string messageId)
    {
        return _messageIds.Contains(messageId);
    }
    
    protected abstract void SetData(TData data);
}