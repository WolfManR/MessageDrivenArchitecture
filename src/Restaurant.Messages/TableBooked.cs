namespace Restaurant.Messages;

public record TableBooked(Guid OrderId, int TableId);