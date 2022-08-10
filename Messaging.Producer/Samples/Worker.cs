using RabbitMQ.Client;

namespace Messaging.Producer.Samples;

/// <summary>
/// Will send messages that will be handled by only one receiver that take message first
/// </summary>
internal class Worker
{
    private readonly NotifyProvider _provider;
    private static IBasicProperties _messageProperties = null!;
    private const string Queue = "task_queue";
    
    public Worker(NotifyProvider provider)
    {
        _provider = provider;
        
        var taskChannel = provider.ConfigureChannel(Queue);
        taskChannel.QueueDeclare(
            queue: Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        
        var taskProperties = _messageProperties = taskChannel.CreateBasicProperties();
        taskProperties.Persistent = true;
    }

    public void DoWork()
    {
        const string message = "Some text";
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            _provider.Send(Queue, Queue, message, properties: _messageProperties);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
}