namespace Restaurant.Kitchen.Tests;

public class PreorderDishConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly TestWriter _writer;

    public PreorderDishConsumerTests(ITestOutputHelper output)
    {
        _writer = new TestWriter(output);
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<PreorderDishConsumer>(); })
            .AddLogging()
            .AddTransient<Chef>()
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
    public async Task ConsumeRequest()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish(
            (IBookingRequest)new BookingRequest(
                orderId,
                Guid.NewGuid(),
                Dish.Pasta,
                DateTime.Now,
                3,
                1));

        Assert.True(await _harness.Consumed.Any<IBookingRequest>());
    }
    
    [Fact]
    public async Task PublishDishReadyOnConsume()
    {
        var orderId = NewId.NextGuid();
        var bus = _harness.Bus;

        await bus.Publish((IBookingRequest)
            new BookingRequest(
                orderId,
                Guid.NewGuid(),
                Dish.Pasta,
                DateTime.Now,
                3,
                1));

        Assert.Contains(_harness.Consumed.Select<IBookingRequest>(), x => x.Context.Message.OrderId == orderId);

        Assert.Contains(_harness.Published.Select<IDishReady>(), x => x.Context.Message.OrderId == orderId);
    }
}