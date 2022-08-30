namespace Restaurant.Messages;

public record BookingRequest(Guid OrderId, Guid ClientId, Dish? PreOrder, DateTime CreationDate, int IncomeTime, int CountOfPersons);