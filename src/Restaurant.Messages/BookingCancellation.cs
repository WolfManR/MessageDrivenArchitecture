namespace Restaurant.Messages;

public record BookingCancellation(Guid OrderId, int? TableId);