using Restaurant.Contracts;

namespace Restaurant.Messages;

public record BookingOrder(Guid OrderId, Guid ClientId, Dish? PreOrder, DateTime CreationDate, int IncomeTime, int CountOfPersons);