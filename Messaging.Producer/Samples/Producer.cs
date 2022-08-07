using RabbitMQ.Client;

namespace Messaging.Producer.Samples;

/// <summary>
/// Will send multiple notifications that will be received by multiple receivers
/// </summary>
internal class Producer
{
    private readonly NotifyProvider _provider;
    private const string Exchange = "logs";
    
    public Producer(NotifyProvider provider)
    {
        _provider = provider;

        provider.ConfigureChannel(Exchange).ExchangeDeclare(exchange: Exchange, type: ExchangeType.Fanout);
    }
    
    public void DoWork()
    {
        const string message = "Some text";
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            _provider.Send(string.Empty, message, Exchange);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
}