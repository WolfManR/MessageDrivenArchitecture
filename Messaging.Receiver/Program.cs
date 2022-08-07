using System.Text;
using Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using var provider = NotifyProvider.Create(new MessagingConfiguration());
provider.ConfigureChannel("hello").QueueDeclare(queue: "hello",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var taskChannel = provider.ConfigureChannel("task_queue");
taskChannel.QueueDeclare(queue: "task_queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
taskChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var (queue, consumer, isAutoAck) = TaskQueue(taskChannel);
provider.StartReceiving(queue, isAutoAck, consumer);

Console.WriteLine(" [*] Waiting for messages.");
while (!Console.KeyAvailable) { }

static (string queue, IBasicConsumer consumer, bool isAutoAck) HelloWorld(IModel channel)
{
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, e) =>
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine(" [x] Received {0}", message);
    };
    
    return ("hello", consumer, true);
}

static (string queue, IBasicConsumer consumer, bool isAutoAck) TaskQueue(IModel channel)
{
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (sender, e) =>
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine(" [x] Received {0}", message);

        int dots = message.Split('.').Length - 1;
        Thread.Sleep(dots * 1000);

        Console.WriteLine(" [x] Done");

        // Note: it is possible to access the channel via
        //       ((EventingBasicConsumer)sender).Model here
        channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
    };

    return ("task_queue", consumer, false);
} 