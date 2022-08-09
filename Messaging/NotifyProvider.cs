using System.Text;
using RabbitMQ.Client;

namespace Messaging;

/// <summary>
/// Notifications provider of rabbit mq
/// </summary>
public class NotifyProvider : IDisposable
{
    private readonly IConnection _connection;

    private readonly Dictionary<string, IModel> _channels = new();

    private NotifyProvider(IConnection connection)
    {
        _connection = connection;
    }
    
    /// <summary>
    /// Create Notify provider with connection configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns>Notify provider</returns>
    public static NotifyProvider Create(MessagingConfiguration configuration)
    {
        ConnectionFactory factory = new()
        {
            HostName = configuration.Host,
            Port = configuration.Port,
            UserName = configuration.Username,
            Password = configuration.Password
        };
        return new(factory.CreateConnection());
    }

    /// <summary>
    /// Creates channel and cache it
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public IModel ConfigureChannel(string identifier)
    {
        if (_channels.TryGetValue(identifier, out var existChannel)) return existChannel;
        
        var channel = _connection.CreateModel();
        if(_channels.TryAdd(identifier, channel)) return channel;
        channel.Dispose();
        return _channels[identifier];
    }

    /// <summary>
    /// Send message to message bus service
    /// </summary>
    /// <param name="channelIdentifier"></param>
    /// <param name="queue"></param>
    /// <param name="message"></param>
    /// <param name="exchange"></param>
    /// <param name="properties"></param>
    /// <exception cref="InvalidOperationException">
    /// throws if channel with <paramref name="channelIdentifier"/> not cached
    /// </exception>
    public void Send(
        string channelIdentifier,
        string queue,
        string message,
        string? exchange = null,
        IBasicProperties? properties = null)
    {
        if (!_channels.TryGetValue(channelIdentifier, out var channel)) 
            throw new InvalidOperationException("You not configure this channel");
        
        channel.BasicPublish(
            exchange: exchange ?? string.Empty,
            routingKey: queue,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message));
    }

    /// <summary>
    /// Start receiving messages from message bus service using <paramref name="consumer"/>
    /// </summary>
    /// <param name="channelIdentifier"></param>
    /// <param name="queue"></param>
    /// <param name="isAutoAck"></param>
    /// <param name="consumer"></param>
    /// <exception cref="InvalidOperationException">
    /// throws if channel with <paramref name="channelIdentifier"/> not cached
    /// </exception>
    public void StartReceiving(
        string channelIdentifier,
        string queue,
        bool isAutoAck,
        IBasicConsumer? consumer)
    {
        if (!_channels.TryGetValue(channelIdentifier, out var channel)) 
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