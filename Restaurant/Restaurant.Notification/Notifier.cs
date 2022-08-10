using System.Collections.Concurrent;

namespace Restaurant.Notification;

public class Notifier
{
    private readonly ILogger<Notifier> _logger;
    private readonly ConcurrentDictionary<Guid, Tuple<Guid?, Accepted>> _orders = new ();

    public Notifier(ILogger<Notifier> logger)
    {
        _logger = logger;
    }
    
    public void Accept(Guid orderId, Accepted accepted, Guid? clientId = null)
    {
        _orders.AddOrUpdate(orderId, new Tuple<Guid?, Accepted>(clientId, accepted),
            (_, oldValue) => new(oldValue.Item1 ?? clientId, oldValue.Item2 | accepted));
            
        Notify(orderId);
    }
    
    private void Notify(Guid orderId)
    {
        var (clientId, orderState) = _orders[orderId];
            
        switch (orderState)
        {
            case Accepted.All:
                _logger.LogInformation("Клиент {0}, блюдо подано на ваш столик", clientId);
                _orders.Remove(orderId, out _);
                break;
            case Accepted.Rejected:
                _logger.LogInformation("Гость {0}, к сожалению, все столики заняты", clientId);
                _orders.Remove(orderId, out _);
                break;
            case Accepted.Booking:
                _logger.LogInformation("Успешно забронировано для клиента {0}", clientId);
                break;
        }
    }
}