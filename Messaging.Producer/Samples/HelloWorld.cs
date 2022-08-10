namespace Messaging.Producer.Samples;

/// <summary>
/// Launch one message and shutdown
/// </summary>
internal class HelloWorld
{
    private readonly NotifyProvider _provider;
    private const string Queue = "hello";

    public HelloWorld(NotifyProvider provider)
    {
        _provider = provider;
        
        provider.ConfigureChannel(Queue).QueueDeclare(
            queue: Queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void DoWork()
    {
        const string message = "Hello World!";
        _provider.Send(Queue, Queue, message);
        Console.WriteLine(" [x] Sent {0}", message);
    }
}