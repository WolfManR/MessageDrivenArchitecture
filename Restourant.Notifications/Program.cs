using System.Collections.Concurrent;
using System.Text;
using Messaging;
using RabbitMQ.Client.Events;

const string notificationQueue = "restaurant_notifications";

using var provider = NotifyProvider.Create(new MessagingConfiguration());

var channel = provider.ConfigureChannel(notificationQueue);
channel.QueueDeclare(
    queue: notificationQueue,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += HandleNotification;

provider.StartReceiving(notificationQueue, false, consumer);

Console.WriteLine(" [*] Ожидаем уведомлений из ресторана. Нажмите любую кнопку чтобы завершить работу...");
while (!Console.KeyAvailable)
{
    await Task.Delay(300);
    if(!NotifyQueue.HasNotifications) continue;

    foreach (var notification in NotifyQueue.GetNotifications())
        Console.WriteLine(notification);
}

static void HandleNotification(object? sender, BasicDeliverEventArgs e)
{
    if (sender is not EventingBasicConsumer { Model: { } channel })
        throw new InvalidOperationException("This must never be thrown");

    var message = Encoding.UTF8.GetString(e.Body.Span);
    NotifyQueue.AddMessage("Уведомление: " + message);

    channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
}

static class NotifyQueue
{
    private static ConcurrentQueue<string> _notifications = new();

    public static bool HasNotifications => _notifications.Count > 0;

    public static void AddMessage(string message)
    {
        _notifications.Enqueue(message);
    }

    public static IEnumerable<string> GetNotifications()
    {
        while (_notifications.TryDequeue(out var notification))
        {
            yield return notification;
        }
    }
}