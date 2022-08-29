namespace Restaurant.Messages;

public record DishOrderApproved
{
    public Guid OrderId { get; set; }
}