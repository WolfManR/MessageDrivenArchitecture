namespace Restaurant.Contracts;

public record BookingRequest(int CountOfPersons, Dish? DishPreOrder, Guid ClientId, int IncomeTime);
public record CancelBookingRequest(Guid OrderId, Guid ClientId);