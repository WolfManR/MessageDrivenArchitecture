using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Booking;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Hall>()
    .AddSingleton<Manager>();

builder.Services.AddMassTransit(x => x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context)));

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("/Book", static async (int countOfPersons, Dish? dish, [FromServices] Manager manager, [FromServices] IBus messageBus) =>
{
    if (!await manager.BookFreeTable(countOfPersons))
    {
        return Results.BadRequest("Нет свободного столика на указанное число мест");
    }
    
    TableBooked order = new(NewId.NextGuid(), NewId.NextSequentialGuid(), true, dish);
    await messageBus.Publish(order);
    return Results.Ok(order.OrderId);
});

app.MapPost("/Free/{orderId}", static (Guid orderId, Manager manager) => Results.Problem("Not released feature"));

app.Run();