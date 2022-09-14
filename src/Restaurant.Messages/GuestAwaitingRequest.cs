namespace Restaurant.Messages;

public record GuestAwaitingRequest(Guid OrderId, Guid ClientId, int IncomeTime, int? TableId);