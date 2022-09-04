using Refit;

namespace Restaurant.Contracts;

public interface IBookingClient
{
    [Get("/tables")]
    Task<IReadOnlyList<TableResponse>> Tables();

    [Post("/book")]
    Task<BookingResponse> Book([Body] BookingRequest request);

    [Post("/free")]
    Task Free([Body] CancelBookingRequest request);
}