using System.Reflection;
using MassTransit;
using Restaurant.Notification;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    
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
        
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<Notifier>();

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();

app.Run();