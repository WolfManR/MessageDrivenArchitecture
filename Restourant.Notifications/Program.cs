using System.Collections.Concurrent;
using System.Text;
using Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string notificationExchange = "restaurant_notifications";
string queueName;

using var provider = NotifyProvider.Create(new MessagingConfiguration());

// configure exchange channel for notifications
var channel = provider.ConfigureChannel(notificationExchange);
channel.ExchangeDeclare(exchange: notificationExchange, type: ExchangeType.Fanout);
queueName  = channel.QueueDeclare().QueueName;
channel.QueueBind(
    queue: queueName,
    exchange: notificationExchange,
    routingKey: string.Empty);

// start consuming notifications from channel
var consumer = new EventingBasicConsumer(channel);
consumer.Received += HandleNotification;
provider.StartReceiving(notificationExchange, queueName, true, consumer);

// print notifications in console with delay in 300 milliseconds
Console.WriteLine(" [*] Ожидаем уведомлений из ресторана. Нажмите любую кнопку чтобы завершить работу...");
while (!Console.KeyAvailable)
{
    await Task.Delay(300);
    if(NotifyQueue.IsEmpty) continue;

    foreach (var notification in NotifyQueue.GetNotifications())
        Console.WriteLine(notification);
}

static void HandleNotification(object? sender, BasicDeliverEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Body.Span);
    NotifyQueue.AddMessage("Уведомление: " + message);
}

static class NotifyQueue
{
    private static readonly ConcurrentQueue<string> Notifications = new();

    public static bool IsEmpty => Notifications.IsEmpty;

    public static void AddMessage(string message)
    {
        Notifications.Enqueue(message);
    }

    public static IEnumerable<string> GetNotifications()
    {
        while (Notifications.TryDequeue(out var notification))
        {
            yield return notification;
        }
    }
}