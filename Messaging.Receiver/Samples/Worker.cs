using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Receiver.Samples;

/// <summary>
/// Will receive messages that will be handled by only one receiver that take message first
/// </summary>
internal class Worker
{
    private readonly NotifyProvider _provider;
    private readonly IModel _channel;
    private const string Queue = "hello";

    public Worker(NotifyProvider provider)
    {
        _provider = provider;
        
        var taskChannel = _channel = provider.ConfigureChannel("task_queue");
        taskChannel.QueueDeclare(queue: "task_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        taskChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    public void Subscribe()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += HandleMessage;
        _provider.StartReceiving(Queue, Queue, false, consumer);
    }

    private void HandleMessage(object? sender, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine(" [x] Received {0}", message);

        int dots = message.Split('.').Length - 1;
        Thread.Sleep(dots * 1000);

        Console.WriteLine(" [x] Done");

        // Note: it is possible to access the channel via
        //       ((EventingBasicConsumer)sender).Model here
        _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
    }
}