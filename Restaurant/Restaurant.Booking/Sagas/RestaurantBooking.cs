using MassTransit;

namespace Restaurant.Booking.Sagas;

public class RestaurantBooking : SagaStateMachineInstance
{
    //идентификатор для соотнесения всех сообщений друг с другом
    public Guid CorrelationId { get; set; }
    
    //текущее состояние саги (по умолчанию присутствуют Initial - 1 и Final - 2)
    public int CurrentState { get; set; }

    //идентификатор заказа/бронирования
    public Guid OrderId { get; set; } 
        
    //идентификатор клиента
    public Guid ClientId { get; set; } 
        
    //маркировка для "композиции" событий (наш случай с кухней и забронированным столом)
    public int ReadyEventStatus { get; set; }
        
    // пометка о том, что наша заявка просрочена
    public Guid? ExpirationId { get; set; }

    // пометка о том, что гость собирается прийти
    public Guid? GuestIncomeId { get; set; }
    
    public int IncomeTime { get; set; }
    public int? TableId { get; set; }
}