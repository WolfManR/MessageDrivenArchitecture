using MassTransit;
using Restaurant.Booking.Messages;
using Restaurant.Messages;

namespace Restaurant.Booking.Sagas;

public sealed class RestaurantBookingSaga : MassTransitStateMachine<RestaurantBooking>
{
    private readonly ILogger<RestaurantBookingSaga> _logger;

    public RestaurantBookingSaga(ILogger<RestaurantBookingSaga> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Event(() => BookingRequested,
            x =>
                x.CorrelateById(context => context.Message.OrderId)
                    .SelectId(context => context.Message.OrderId));

        Event(() => TableBooked,
            x =>
                x.CorrelateById(context => context.Message.OrderId));

        Event(() => DishReady,
            x =>
                x.CorrelateById(context => context.Message.OrderId));

        CompositeEvent(() => BookingApproved,
            x => x.ReadyEventStatus, DishReady, TableBooked);

        Event(() => BookingRequestFault,
            x =>
                x.CorrelateById(m => m.Message.Message.OrderId));

        Schedule(() => BookingExpired,
            x => x.ExpirationId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(30);
                x.Received = e => e.CorrelateById(context => context.Message.OrderId);
            });

        Schedule(() => GuestIncome,
            x => x.GuestIncomeId, x =>
            {
                x.Received = e => e.CorrelateById(context => context.Message.OrderId);
            });

        Initially(
            When(BookingRequested)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.OrderId;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.ClientId = context.Message.ClientId;

                    context.Saga.IncomeTime = context.Message.IncomeTime;
                    
                    _logger.LogInformation("Saga: {CreationDate}", context.Message.CreationDate);
                })
                .Schedule(BookingExpired, context => new BookingExpire(context.Saga))
                .TransitionTo(AwaitingBookingApproved)
        );

        During(AwaitingBookingApproved,
            When(TableBooked)
                .Then(context => context.Saga.TableId = context.Message.TableId),
            When(BookingApproved)
                .Unschedule(BookingExpired)
                .Publish(context =>
                    (INotify)new Notify(context.Saga.OrderId,
                        context.Saga.ClientId,
                        "Стол успешно забронирован"))
                .Schedule(GuestIncome,
                    context => new GuestIncome(context.Saga),
                    context => TimeSpan.FromSeconds(context.Saga.IncomeTime))
                .Publish(context =>
                    (INotify)new Notify(context.Saga.OrderId,
                        context.Saga.ClientId,
                        "Ожидание гостя"))
                .TransitionTo(AwaitingGuest),
            When(BookingRequestFault)
                .Then(context => _logger.LogError("Ошибочка вышла! Заказ отменяется {Order}", context.Saga.OrderId))
                .Publish(context => (INotify)new Notify(context.Saga.OrderId,
                    context.Saga.ClientId,
                    "Приносим извинения, стол забронировать не получилось."))
                .Publish(context => (IBookingCancellation)
                    new BookingCancellation(context.Message.Message.OrderId, context.Saga.TableId))
                .Finalize(),
            When(BookingExpired.Received)
                // TODO: Ignore events
                .Then(context => 
                    _logger.LogWarning(
                        "Отмена заказа {Order}, слишком долго исполнялся",
                        context.Saga.OrderId))
                .Publish(context => (IBookingCancellation)
                    new BookingCancellation(context.Message.OrderId, context.Saga.TableId))
                .Finalize()
        );
        
        During(AwaitingGuest,
            When(GuestIncome.Received)
                .Publish(context =>
                    (INotify)new Notify(context.Saga.OrderId,
                        context.Saga.ClientId,
                        "Гость прибыл"))
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State AwaitingBookingApproved { get; private set; }
    public Event<IBookingRequest> BookingRequested { get; private set; }
    public Event<ITableBooked> TableBooked { get; private set; }
    public Event<IDishReady> DishReady { get; private set; }

    public Event<Fault<IBookingRequest>> BookingRequestFault { get; private set; }

    public Schedule<RestaurantBooking, IBookingExpire> BookingExpired { get; private set; }
    public Event BookingApproved { get; private set; }
    
    
    public State AwaitingGuest { get; private set; }
    
    public Schedule<RestaurantBooking, IGuestIncome> GuestIncome { get; private set; }
}