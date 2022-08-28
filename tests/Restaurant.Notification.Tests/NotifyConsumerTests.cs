namespace Restaurant.Notification.Tests;

public class NotifyConsumerTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly TestWriter _writer;

    public NotifyConsumerTests(ITestOutputHelper output)
    {
        _writer = new TestWriter(output);
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg => { cfg.AddConsumer<NotifyConsumer>(); })
            .AddLogging()
            .AddSingleton<Notifier>()
            .AddSingleton<IRepository<Notify>, InMemoryRepository<Notify>>()
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
            (INotify)new Notify(
                orderId,
                Guid.NewGuid(),
                "Hello World"));

        Assert.True(await _harness.Consumed.Any<INotify>());
    }
}