using Restaurant.Contracts;

namespace BlazorApp.Models;

public class BookingClientMock : IBookingClient
{
    private readonly List<TableResponse> _tables = new()
    {
        new TableResponse(TableState.Free,1,2),
        new TableResponse(TableState.Free,2,3),
        new TableResponse(TableState.Free,3,4),
        new TableResponse(TableState.Free,4,5),
        new TableResponse(TableState.Free,5,3),
        new TableResponse(TableState.Free,6,1),
    };

    private readonly List<Order> _orders = new();

    public async Task<IReadOnlyList<TableResponse>> Tables() => _tables;

    public async Task<BookingResponse> Book(BookingRequest request)
    {
        var freeTable = _tables.FirstOrDefault(t => t.State == TableState.Free && t.SeatsCount == request.CountOfPersons);
        // This is wrong behavior and will generate plenty of booking orders without possibility to completed
        Order order = new(Guid.NewGuid(), request.ClientId, request.DishPreOrder);
        if (freeTable is not null)
        {
            var index = _tables.IndexOf(freeTable);
            _tables[index] = freeTable with { State = TableState.Booked };
            order = order with {TableId = freeTable.Id};
        }
        _orders.Add(order);
        return new BookingResponse(order.OrderId);
    }

    public async Task Free(CancelBookingRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.ClientId == request.ClientId && o.OrderId == request.OrderId);
        if (order is null) throw new InvalidOperationException("Order not exist");
        order.OrderState = OrderState.Canceled;
        if(order.TableId is null) return;
        var table = _tables.FirstOrDefault(t => t.Id == order.TableId);
        var index = _tables.IndexOf(table);
        _tables[index] = table with { State = TableState.Free };
    }

    record Order(Guid OrderId, Guid ClientId, Dish? Dish = null, int? TableId = null, OrderState OrderState = OrderState.Process)
    {
        public Dish? Dish { get; init; } = Dish;
        public int? TableId { get; init; } = TableId;
        public OrderState OrderState { get; set; } = OrderState;
    }

    public enum OrderState{Process,Canceled,Completed}
}