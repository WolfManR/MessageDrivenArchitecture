using Microsoft.AspNetCore.Mvc;
using Restaurant.Booking;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services
    .AddSingleton<Hall>()
    .AddSingleton<Manager>();

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("/Book", static async (int countOfPersons, Dish? dish, [FromServices] Manager manager) =>
{
    if(await manager.BookFreeTable(countOfPersons)) return Results.Ok();
    return Results.BadRequest("Нет свободного столика на указанное число мест");
});

app.MapPost("/Free/{orderId}", static (Guid orderId, Manager manager) => Results.Problem("Not released feature"));

app.Run();