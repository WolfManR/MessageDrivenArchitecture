namespace Restaurant.Booking.Sagas;

public class BookingSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }
    public int ReadyEventStatus { get; set; }
    public Guid? ExpirationId { get; set; }
    
    public Guid OrderId { get; set; }
    public Guid ClientId { get; set; }
    public int? TableId { get; set; }
    public int IncomeTime { get; set; }
}