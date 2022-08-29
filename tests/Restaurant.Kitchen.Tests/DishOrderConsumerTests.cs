namespace Restaurant.Kitchen.Tests;

public class DishOrderConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly TestWriter _writer;

    public DishOrderConsumerTests(ITestOutputHelper output)
    {
        _writer = new TestWriter(output);
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<DishOrderConsumer>(); })
            .AddLogging()
            .AddTransient<Chef>()
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
    public async Task ConsumeRequest()
    {
        var orderId = Guid.NewGuid();

        await _harness.Bus.Publish((IDishOrder)new DishOrder(orderId, Dish.Pasta));

        Assert.True(await _harness.Consumed.Any<IDishOrder>());
    }
    
    [Fact]
    public async Task PublishDishReadyOnConsume()
    {
        var orderId = NewId.NextGuid();
        
        await _harness.Bus.Publish((IDishOrder)new DishOrder(orderId, Dish.Pasta));

        Assert.Contains(_harness.Consumed.Select<IDishOrder>(), x => x.Context.Message.OrderId == orderId);

        Assert.Contains(_harness.Published.Select<IDishReady>(), x => x.Context.Message.OrderId == orderId);
    }
}