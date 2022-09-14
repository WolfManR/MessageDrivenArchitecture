using Restaurant.Booking.Sagas;
using Restaurant.Contracts;
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
                
                x.AddSagaStateMachine<BookingSaga, BookingSagaState>()
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
        
        await _harness.Bus.Publish(new BookingOrder(
            orderId,
            clientId,
            Dish.Pasta,
            DateTime.Now,
            3,
            1));
        
        Assert.True(await _harness.Published.Any<BookingOrder>());
        Assert.True(await _harness.Consumed.Any<BookingOrder>());

        var sagaHarness = _harness.GetSagaStateMachineHarness<BookingSaga, BookingSagaState>();

        Assert.True(await sagaHarness.Consumed.Any<BookingOrder>());
        Assert.True(await sagaHarness.Created.Any(x => x.CorrelationId == orderId));

        var saga = sagaHarness.Created.Contains(orderId);

        Assert.NotNull(saga);
        Assert.Equal(saga.ClientId, clientId);
        Assert.True(await _harness.Published.Any<TableBooked>());
        Assert.True(await _harness.Published.Any<DishReady>());
        Assert.True(await _harness.Published.Any<Notify>());
        Assert.Equal(3, saga.CurrentState);
    }
}