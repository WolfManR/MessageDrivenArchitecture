using Restaurant.Booking.Sagas;
using Restaurant.Kitchen;
using Restaurant.Kitchen.Consumers;

namespace Restaurant.Booking.Tests;

public class BookingRequestSagaTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly TestWriter _writer;

    public BookingRequestSagaTests(ITestOutputHelper output)
    {
        _writer = new TestWriter(output);
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<PreorderDishConsumer>();
                cfg.AddConsumer<RestaurantBookingRequestConsumer>();
                
                cfg.AddSagaStateMachine<RestaurantBookingSaga, RestaurantBooking>()
                    .InMemoryRepository();
                cfg.AddDelayedMessageScheduler();
            })
            .AddLogging()
            .AddTransient<Hall>()
            .AddTransient<Chef>()
            .AddTransient<Manager>()
            .AddSingleton<IRepository<BookingRequest>, InMemoryRepository<BookingRequest>>()
            .BuildServiceProvider(true);

        _harness = _provider.GetTestHarness();
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
    public async Task Test()
    {
        var orderId = NewId.NextGuid();
        var clientId = NewId.NextGuid();
        
        await _harness.Bus.Publish(new BookingRequest(
            orderId,
            clientId,
            Dish.Pasta,
            DateTime.Now,
            3,
            1));
        
        Assert.True(await _harness.Published.Any<IBookingRequest>());
        Assert.True(await _harness.Consumed.Any<IBookingRequest>());

        var sagaHarness = _provider
            .GetRequiredService<ISagaStateMachineTestHarness<RestaurantBookingSaga, RestaurantBooking>>();

        Assert.True(await sagaHarness.Consumed.Any<IBookingRequest>());
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId));

        var saga = sagaHarness.Created.Contains(orderId);

        Assert.NotNull(saga);
        Assert.Equal(saga.ClientId, clientId);
        Assert.True(await _harness.Published.Any<ITableBooked>());
        Assert.True(await _harness.Published.Any<IDishReady>());
        // BookingExpire block
        // Assert.True(await _harness.Published.Any<INotify>());
        Assert.Equal(3, saga.CurrentState);
    }
}