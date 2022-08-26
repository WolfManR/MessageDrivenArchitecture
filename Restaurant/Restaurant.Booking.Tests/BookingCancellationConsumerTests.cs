namespace Restaurant.Booking.Tests;

public class BookingCancellationConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly TestWriter _writer;
    private readonly Hall _restaurantHall;

    public BookingCancellationConsumerTests(ITestOutputHelper output)
    {
        _writer = new TestWriter(output);
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<BookingCancellationConsumer>(); })
            .AddLogging()
            .AddSingleton<Hall>()
            .AddSingleton<Manager>()
            .AddSingleton<IRepository<BookingCancellation>, InMemoryRepository<BookingCancellation>>()
            .BuildServiceProvider(true);

        _harness = _provider.GetTestHarness();
        _restaurantHall = _provider.GetService<Hall>()!;
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.OutputTimeline(_writer, options => options.Now().IncludeAddress());
        await _provider.DisposeAsync();
    }
    
    [Fact]
    public async Task ConsumeRequest()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish((IBookingCancellation)new BookingCancellation(orderId, 1));

        Assert.True(await _harness.Consumed.Any<IBookingCancellation>());
    }

    [Fact]
    public async Task FreeBookedTable()
    {
        var orderId = Guid.NewGuid();
        var table = _restaurantHall.FindFreeTable(1);
        table!.Set(TableState.Booked);
        var tableId = table.Id;
        
        await _harness.Bus.Publish((IBookingCancellation)new BookingCancellation(orderId, tableId));

        Assert.True(await _harness.Consumed.Any<IBookingCancellation>());
        Assert.Equal(TableState.Free, table.State);
    }
}