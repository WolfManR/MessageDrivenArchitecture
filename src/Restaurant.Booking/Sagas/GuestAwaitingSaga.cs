namespace Restaurant.Booking.Sagas;

public class GuestAwaitingSaga : MassTransitStateMachine<GuestAwaitingSagaState>
{
    public GuestAwaitingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => GuestAwaitingRequested, x => x.CorrelateById(c => c.Message.OrderId));
        Event(() => BookingCancellationRequested, x => x.CorrelateById(c => c.Message.OrderId));

        Schedule(() => GuestIncome, x => x.GuestIncomeId, x => { x.Received = e => e.CorrelateById(c => c.Message.OrderId); });

        Initially(
            When(GuestAwaitingRequested)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.ClientId = context.Message.ClientId;
                    context.Saga.IncomeTime = context.Message.IncomeTime;
                    context.Saga.TableId = context.Message.TableId;
                })
                .Schedule(GuestIncome,
                    context => new GuestIncome(context.Saga.OrderId),
                    context => TimeSpan.FromSeconds(context.Saga.IncomeTime))
                .Publish(context => new Notify(
                    context.Saga.OrderId,
                    context.Saga.ClientId,
                    "Ожидание гостя"))
                .TransitionTo(AwaitingGuest));

        During(AwaitingGuest,
            When(GuestIncome.Received)
                .Publish(context => new Notify(
                    context.Saga.OrderId,
                    context.Saga.ClientId,
                    "Гость прибыл"))
                .Finalize());

        DuringAny(
            When(BookingCancellationRequested)
                .Publish(context => new BookingCancellation(context.Message.OrderId, context.Saga.TableId))
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State AwaitingGuest { get; set; }

    public Event<GuestAwaitingRequest> GuestAwaitingRequested { get; set; }
    public Event<ClientBookingCancellation> BookingCancellationRequested { get; set; }

    public Schedule<GuestAwaitingSagaState, GuestIncome> GuestIncome { get; set; }
}