using MassTransit;
using MassTransit.Audit;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Restaurant.Booking.Sagas;
using Restaurant.MassTransit;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Hall>()
    .AddSingleton<Manager>()
    .AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));

builder.Services
    .AddTransient<RestaurantBooking>()
    .AddTransient<RestaurantBookingSaga>()
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

    x.AddSagaStateMachine<RestaurantBookingSaga, RestaurantBooking>()
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

app.MapPost("/Book", static async (int countOfPersons, Dish? dish, [FromServices] Manager manager, [FromServices] IBus messageBus) =>
{
    BookingRequest order = new(
        orderId: NewId.NextGuid(),
        clientId: NewId.NextSequentialGuid(),
        preOrder: dish,
        creationDate: DateTime.UtcNow,
        incomeTime: Random.Shared.Next(7, 15),
        countOfPersons: countOfPersons);
    await messageBus.Publish<IBookingRequest>(order);
    return Results.Ok(order.OrderId);
});

app.MapPost("/Free/{orderId}", static (Guid orderId, Manager manager) => Results.Problem("Not released feature"));

app.MapMetrics();

app.Run();