using System.Diagnostics;
using System.Text;
using ConsoleMenu;
using Refit;

Console.OutputEncoding = Encoding.UTF8;
var bookingClient = RestService.For<IBookingApi>("");

Menu mainMenu = new();
mainMenu.SubTitle = "Привет! Желаете забронировать или освободить столик?";
mainMenu.AddMenuItem("Забронировать", BookTable);
mainMenu.AddMenuItem("Освободить", FreeTable);
mainMenu.AddMenuItem("Выход", mainMenu.Close);
mainMenu.ShowMenu();

async void BookTable()
{
    Console.WriteLine("На сколько мест столик желаете:");
    int.TryParse(Console.ReadLine(), out var places);
    var orderId = await bookingClient.Book(places);
    State.OrderId = orderId;
}

async void FreeTable()
{
    if (State.OrderId == Guid.Empty)
    {
        Console.WriteLine("Вы ещё не забронировали столик");
        return;
    }

    await bookingClient.Free(State.OrderId);
    State.OrderId = Guid.Empty;
}

static void LogExecuteTime(Action execution)
{
    var stopWatch = new Stopwatch();
    stopWatch.Start();

    execution.Invoke();

    stopWatch.Stop();
    var ts = stopWatch.Elapsed;
    Console.WriteLine($"{ts.Seconds:80}:{ts.Milliseconds:00}");
}

static class State
{
    public static Guid OrderId { get; set; }
}

    

interface IBookingApi
{
    [Post("")]
    Task<Guid> Book(int countOfPersons);

    [Post("")]
    Task Free(Guid orderId);
}