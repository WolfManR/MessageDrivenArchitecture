namespace Restaurant.Messages;

public record ClientBookingCancellation(Guid OrderId, Guid ClientId);