﻿using MassTransit;
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

        Event(
            () => BookingRequested,
            x => x.CorrelateById(context => context.Message.OrderId)
                .SelectId(context => context.Message.OrderId));

        Event(() => TableBooked, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => DishOrderApproved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => BookingCancellationRequested, x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => BookingRequestFault, x => x.CorrelateById(m => m.Message.Message.OrderId));

        Schedule(() => BookingExpired, x => x.ExpirationId, x =>
        {
            x.Delay = TimeSpan.FromSeconds(30);
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
                .Publish(context => (IDishOrder)new DishOrder(
                    context.Message.OrderId,
                    context.Message.PreOrder))
                .Schedule(BookingExpired, context => new BookingExpire(context.Saga))
                .TransitionTo(AwaitingBookingApproved)
        );

        During(AwaitingBookingApproved,
            When(TableBooked)
                .Then(context => context.Saga.TableId = context.Message.TableId),
            When(DishOrderApproved)
                .Publish(context =>
                    (INotify)new Notify(
                        context.Saga.OrderId,
                        context.Saga.ClientId,
                        "К вашему приходу блюдо будет готово")));

        CompositeEvent(() => BookingApproved, x => x.ReadyEventStatus, TableBooked, DishOrderApproved);

        DuringAny(
            When(BookingApproved)
                .Unschedule(BookingExpired)
                .Publish(context =>
                    (INotify)new Notify(context.Saga.OrderId,
                        context.Saga.ClientId,
                        "Стол успешно забронирован"))
                .Publish(context => new GuestAwaitingRequest(
                    context.Saga.OrderId,
                    context.Saga.ClientId,
                    context.Saga.IncomeTime,
                    context.Saga.TableId))
                .Finalize(),
            When(BookingRequestFault)
                .Unschedule(BookingExpired)
                .Then(context => _logger.LogError("Ошибочка вышла! Заказ отменяется {Order}", context.Saga.OrderId))
                .Publish(context => (INotify)new Notify(
                    context.Saga.OrderId,
                    context.Saga.ClientId,
                    "Приносим извинения, стол забронировать не получилось."))
                .Publish(context => (IBookingCancellation)
                    new BookingCancellation(context.Message.Message.OrderId, context.Saga.TableId))
                .Finalize(),
            When(BookingExpired.Received)
                .Then(context =>
                    _logger.LogWarning(
                        "Отмена заказа {Order}, слишком долго исполнялся",
                        context.Saga.OrderId))
                .Publish(context => (IBookingCancellation)
                    new BookingCancellation(context.Message.OrderId, context.Saga.TableId))
                .Finalize(),
            When(BookingCancellationRequested)
                .Publish(context => (IBookingCancellation)
                    new BookingCancellation(context.Message.OrderId, context.Saga.TableId))
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State AwaitingBookingApproved { get; private set; }
    public Event BookingApproved { get; private set; }

    public Event<IBookingRequest> BookingRequested { get; private set; }
    public Event<ITableBooked> TableBooked { get; private set; }
    public Event<DishOrderApproved> DishOrderApproved { get; private set; }

    public Event<Fault<IBookingRequest>> BookingRequestFault { get; private set; }
    public Event<ClientBookingCancellation> BookingCancellationRequested { get; set; }

    public Schedule<RestaurantBooking, IBookingExpire> BookingExpired { get; private set; }
}