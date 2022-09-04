namespace Restaurant.Booking.Sagas;

public class GuestAwaitingSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    
    public Guid OrderId { get; set; }
    public Guid ClientId { get; set; }
    public int IncomeTime { get; set; }
    public int? TableId { get; set; }
    
    // пометка о том, что гость собирается прийти
    public Guid? GuestIncomeId { get; set; }
}