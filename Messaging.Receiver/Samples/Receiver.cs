using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Receiver.Samples;

/// <summary>
/// Will receive multiple notifications that will be received by multiple receivers
/// </summary>
internal class Receiver
{
    private readonly NotifyProvider _provider;
    private const string Exchange = "logs";
    private readonly string _queue;
    private readonly IModel _channel;

    public Receiver(NotifyProvider provider)
    {
        _provider = provider;

        var channel = _channel = _provider.ConfigureChannel(Exchange);
        channel.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Fanout);

        var queueName = _queue = channel.QueueDeclare().QueueName;
        channel.QueueBind(
            queue: queueName,
            exchange: Exchange,
            routingKey: string.Empty);
    }

    public void Subscribe()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += HandleMessage;
        _provider.StartReceiving(_queue, true, consumer);
    }

    private void HandleMessage(object? model, BasicDeliverEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Body.Span);
        Console.WriteLine(" [x] {0}", message);
    }
}