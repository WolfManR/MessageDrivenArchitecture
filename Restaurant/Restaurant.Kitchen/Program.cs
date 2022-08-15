using MassTransit;
using Restaurant.Kitchen;
using Restaurant.Kitchen.Consumers;
using Restaurant.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PreorderDishConsumer>();

    x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
});

builder.Services.AddSingleton<Chef>();

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.MapPost("order", (Dish dish) => Results.Problem("Not released feature"));

app.Run();