using Restaurant.Contracts;

namespace BlazorApp.Models;

public class BookingRequestVM
{
    public int CountOfPersons { get; set; } = 1;
    public Dish? DishPreOrder { get; set; }
    public int IncomeTime { get; set; } = 10;
}