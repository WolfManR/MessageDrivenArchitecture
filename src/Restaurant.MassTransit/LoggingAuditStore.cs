using System.Text.Json;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;

namespace Restaurant.MassTransit;

public class LoggingAuditStore : IMessageAuditStore
{
    private readonly ILogger<LoggingAuditStore> _logger;

    public LoggingAuditStore(ILogger<LoggingAuditStore> logger)
    {
        _logger = logger;
    }
    
    public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
    {
        _logger.LogInformation("{Metadata}\n{Message}", JsonSerializer.Serialize(metadata), JsonSerializer.Serialize(message));
        return Task.CompletedTask;
    }
}