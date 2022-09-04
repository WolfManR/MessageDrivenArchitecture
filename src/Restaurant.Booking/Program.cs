using MassTransit;
using MassTransit.Audit;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Restaurant.Booking.Sagas;
using Restaurant.Contracts;
using Restaurant.MassTransit;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Hall>()
    .AddSingleton<Manager>()
    .AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));

builder.Services
    .AddTransient<BookingSagaState>()
    .AddTransient<BookingSaga>()
    .AddSingleton<IMessageAuditStore, LoggingAuditStore>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RestaurantBookingRequestConsumer>(cfg =>
        {
            cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            cfg.UseScheduledRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
        })
        .Endpoint(e => e.Temporary = true);
    x.AddConsumer<BookingRequestFaultConsumer>(cfg =>
    {
        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
        cfg.UseScheduledRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
    }).Endpoint(e => e.Temporary = true);
    x.AddConsumer<BookingCancellationConsumer>(cfg =>
    {
        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
        cfg.UseScheduledRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10)));
    }).Endpoint(e => e.Temporary = true);

    x.AddSagaStateMachine<BookingSaga, BookingSagaState>()
        .Endpoint(e => e.Temporary = true)
        .InMemoryRepository();
    x.AddSagaStateMachine<GuestAwaitingSaga, GuestAwaitingSagaState>()
        .Endpoint(e => e.Temporary = true)
        .InMemoryRepository();

    x.AddDelayedMessageScheduler();

    // Bad practice and useless
    var serviceProvider = builder.Services.BuildServiceProvider();
    var auditStore = serviceProvider.GetService<IMessageAuditStore>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Durable = false;
        cfg.UseDelayedMessageScheduler();
        cfg.UseInMemoryOutbox();
        cfg.ConfigureEndpoints(context);

        cfg.ConnectSendAuditObservers(auditStore);
        cfg.ConnectConsumeAuditObserver(auditStore);
        
        cfg.UsePrometheusMetrics(serviceName: "restaurant_booking");
    });
});

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("book", static async ([FromBody] BookingRequest request, [FromServices] IBus messageBus) =>
{
    BookingOrder order = new(
        OrderId: NewId.NextGuid(),
        ClientId: request.ClientId,
        PreOrder: request.DishPreOrder,
        CreationDate: DateTime.UtcNow,
        IncomeTime: request.IncomeTime,
        CountOfPersons: request.CountOfPersons);
    await messageBus.Publish(order);
    return Results.Ok(new {order.OrderId});
});

app.MapPost("free", static async ([FromBody] CancelBookingRequest request, [FromServices] IBus messageBus) =>
{
    ClientBookingCancellation cancellation = new(request.OrderId, request.ClientId);
    await messageBus.Publish(cancellation);
    return Results.Ok();
});

app.MapGet("tables", ([FromServices] Hall hall) => Results.Ok(hall.ListTables()));

app.MapMetrics();

app.Run();

