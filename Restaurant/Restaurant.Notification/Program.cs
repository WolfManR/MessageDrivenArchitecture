using MassTransit;
using MassTransit.Audit;
using Prometheus;
using Restaurant.MassTransit;
using Restaurant.Messages;
using Restaurant.Notification;
using Restaurant.Notification.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Notifier>()
    .AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>))
    .AddSingleton<IMessageAuditStore, LoggingAuditStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotifyConsumer>(cfg =>
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
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(5,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(100),
                TimeSpan.FromSeconds(5));
            r.Ignore<StackOverflowException>();
            r.Ignore<ArgumentNullException>(x => x.Message.Contains("Consumer"));
        });
        
        cfg.UseDelayedMessageScheduler();
        cfg.UseInMemoryOutbox();
        cfg.ConfigureEndpoints(context);
        
        cfg.ConnectSendAuditObservers(auditStore);
        cfg.ConnectConsumeAuditObserver(auditStore);
        
        cfg.UsePrometheusMetrics(serviceName: "restaurant_notification");
    });
});

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapMetrics();

app.Run();