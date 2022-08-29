using Restaurant.Messages;

namespace Restaurant.Kitchen;

public class Chef
{
    private readonly Random _random = Random.Shared;

    public bool CheckDishReady(Guid orderId, Dish? dish)
    {
        // Check ready dish
        return _random.Next(0, 50) > 15;
    }
}