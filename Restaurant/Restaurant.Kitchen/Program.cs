using MassTransit;
using MassTransit.Audit;
using Prometheus;
using Restaurant.Kitchen;
using Restaurant.Kitchen.Consumers;
using Restaurant.MassTransit;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Chef>()
    .AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>))
    .AddSingleton<IMessageAuditStore, LoggingAuditStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingCancellationConsumer>(cfg =>
        {
            cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            cfg.UseScheduledRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
        })
        .Endpoint(e => e.Temporary = true);
    x.AddConsumer<PreorderDishConsumer>(cfg =>
        {
            cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            cfg.UseScheduledRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
        })
        .Endpoint(e => e.Temporary = true);

    x.AddDelayedMessageScheduler();

    // Bad practice and useless
    var serviceProvider = builder.Services.BuildServiceProvider();
    var auditStore = serviceProvider.GetService<IMessageAuditStore>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseDelayedMessageScheduler();
        cfg.UseInMemoryOutbox();
        cfg.ConfigureEndpoints(context);
        
        cfg.ConnectSendAuditObservers(auditStore);
        cfg.ConnectConsumeAuditObserver(auditStore);
        
        cfg.UsePrometheusMetrics(serviceName: "restaurant_kitchen");
    });
});

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("order", (Dish dish) => Results.Problem("Not released feature"));

app.MapMetrics();

app.Run();