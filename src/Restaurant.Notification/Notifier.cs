namespace Restaurant.Notification;

public class Notifier
{
    private readonly ILogger<Notifier> _logger;
    public Notifier(ILogger<Notifier> logger)
    {
        _logger = logger;
    }
    
    public void Notify(Guid orderId, Guid clientId, string message)
    {
        _logger.LogInformation("[Order: {Order}] Уважаемый клиент {Client}!, {Message}", orderId, clientId, message);
    }
}