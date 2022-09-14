namespace Restaurant.Messages;

public record Notify(Guid OrderId, Guid ClientId, string Message);