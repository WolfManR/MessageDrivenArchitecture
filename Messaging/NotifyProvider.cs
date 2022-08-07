using System.Text;
using RabbitMQ.Client;

namespace Messaging;

public class NotifyProvider : IDisposable
{
    private readonly IConnection _connection;

    private readonly Dictionary<string, IModel> _channels = new();

    private NotifyProvider(IConnection connection)
    {
        _connection = connection;
    }
    
    public static NotifyProvider Create(MessagingConfiguration configuration)
    {
        ConnectionFactory factory = new()
        {
            HostName = configuration.Host,
            Port = configuration.Port,
            UserName = configuration.Username,
            Password = configuration.Password
        };
        return new NotifyProvider(factory.CreateConnection());
    }

    public IModel ConfigureChannel(string queue)
    {
        if (_channels.TryGetValue(queue, out var existChannel)) return existChannel;
        
        var channel = _connection.CreateModel();
        if(_channels.TryAdd(queue, channel)) return channel;
        channel.Dispose();
        return _channels[queue];
    }

    public void Send(string queue, string message, string? exchange = null, IBasicProperties? properties = null)
    {
        if (!_channels.TryGetValue(queue, out var channel)) 
            throw new InvalidOperationException("You not configure this channel");
        
        channel.BasicPublish(
            exchange: exchange ?? string.Empty,
            routingKey: queue,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message));
    }

    public void StartReceiving(string queue, bool isAutoAck, IBasicConsumer? consumer)
    {
        if (!_channels.TryGetValue(queue, out var channel)) 
            throw new InvalidOperationException("You not configure this channel");
        
        channel.BasicConsume(
            queue: queue,
            autoAck: isAutoAck,
            consumer: consumer);
    }
    
    public void Dispose()
    {
        foreach (var channel in _channels.Values)
        {
            channel.Dispose();
        }

        _connection.Dispose();
    }
}