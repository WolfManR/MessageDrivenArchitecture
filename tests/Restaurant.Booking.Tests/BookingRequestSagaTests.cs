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
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<DishOrderConsumer>();
                x.AddConsumer<RestaurantBookingRequestConsumer>();
                
                x.AddSagaStateMachine<RestaurantBookingSaga, RestaurantBooking>()
                    .InMemoryRepository();
                x.AddPublishMessageScheduler();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseInMemoryScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            })
            .AddLogging()
            .AddTransient<Hall>()
            .AddTransient<Chef>()
            .AddTransient<Manager>()
            .AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>))
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
    public async Task SagaSucceed()
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

        var sagaHarness = _harness.GetSagaStateMachineHarness<RestaurantBookingSaga, RestaurantBooking>();

        Assert.True(await sagaHarness.Consumed.Any<IBookingRequest>());
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId));

        var saga = sagaHarness.Created.Contains(orderId);

        Assert.NotNull(saga);
        Assert.Equal(saga.ClientId, clientId);
        Assert.True(await _harness.Published.Any<ITableBooked>());
        Assert.True(await _harness.Published.Any<IDishReady>());
        Assert.True(await _harness.Published.Any<INotify>());
        Assert.Equal(3, saga.CurrentState);
    }
}