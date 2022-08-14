using MassTransit;
using Restaurant.Messages;

namespace Restaurant.Kitchen.Consumers;

public class TableBookedConsumer : IConsumer<ITableBooked>
{
    private readonly Chef _chef;

    public TableBookedConsumer(Chef chef)
    {
        _chef = chef;
    }
    
    public Task Consume(ConsumeContext<ITableBooked> context)
    {
        var result = context.Message.Success;
        
        if(result)
            _chef.CheckDishReady(context.Message.OrderId, context.Message.PreOrder);
        
        return context.ConsumeCompleted;
    }
}