using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Booking;
using Restaurant.Booking.Consumers;
using Restaurant.Booking.Sagas;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Hall>()
    .AddSingleton<Manager>();

builder.Services    
    .AddTransient<RestaurantBooking>()
    .AddTransient<RestaurantBookingSaga>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RestaurantBookingRequestConsumer>().Endpoint(e => e.Temporary = true);
    x.AddConsumer<BookingRequestFaultConsumer>().Endpoint(e => e.Temporary = true);

    x.AddSagaStateMachine<RestaurantBookingSaga, RestaurantBooking>()
        .Endpoint(e => e.Temporary = true)
        .InMemoryRepository();
    
    x.AddDelayedMessageScheduler();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseDelayedMessageScheduler();
        cfg.UseInMemoryOutbox();
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("/Book", static async (int countOfPersons, Dish? dish, [FromServices] Manager manager, [FromServices] IBus messageBus) =>
{
    if (!await manager.BookFreeTable(countOfPersons))
    {
        return Results.BadRequest("Нет свободного столика на указанное число мест");
    }
    
    BookingRequest order = new(NewId.NextGuid(), NewId.NextSequentialGuid(), dish, DateTime.UtcNow);
    await messageBus.Publish<IBookingRequest>(order);
    return Results.Ok(order.OrderId);
});

app.MapPost("/Free/{orderId}", static (Guid orderId, Manager manager) => Results.Problem("Not released feature"));

app.Run();