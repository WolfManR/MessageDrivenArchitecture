using Restaurant.Booking.Sagas;

namespace Restaurant.Booking.Messages;

public interface IGuestIncome
{
    Guid OrderId { get; }
    Guid ClientId { get; }
}

public class GuestIncome : IGuestIncome
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }

    public GuestIncome(RestaurantBooking instance)
    {
        OrderId = instance.OrderId;
        ClientId = instance.ClientId;
    }
}