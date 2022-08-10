using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Receiver.Samples;

/// <summary>
/// Receive one message and shutdown
/// </summary>
internal class HelloWorld
{
    private readonly NotifyProvider _provider;
    private readonly IModel _channel;
    private const string Queue = "hello";
    public HelloWorld(NotifyProvider provider)
    {
        _provider = provider;
        
        var channel = _channel = provider.ConfigureChannel(Queue);
        channel.QueueDeclare(
            queue: Queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void Subscribe()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += HandleMessage;
        _provider.StartReceiving(Queue, Queue, true, consumer);
    }

    private void HandleMessage(object? _, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine(" [x] Received {0}", message);
    }
}