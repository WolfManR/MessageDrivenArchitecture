using Restaurant.Contracts;

namespace BlazorApp.Models;

public class OrderVM
{
    public Guid OrderId { get; set; }
    public Guid ClientId { get; set; }
    public Dish? Dish { get; set; }
    public string Error { get; set; } = string.Empty;
}