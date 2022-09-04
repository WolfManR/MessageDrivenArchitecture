using Restaurant.Contracts;

namespace Restaurant.Messages;

public record DishOrder(Guid OrderId, Dish? Dish);