using Messaging;
using RabbitMQ.Client;

public class Hall : IDisposable
{
    private readonly List<Table> _tables = new();
    private readonly PeriodicTimer _timer = new (TimeSpan.FromSeconds(20));
    private readonly CancellationTokenSource _freeTablesCancellationSource = new();
    private readonly AutoResetEvent _event = new(true);
    
    private readonly NotifyProvider _notifyProvider;
    private const string NotificationExchange = "restaurant_notifications";
    
    public Hall()
    {
        for (byte i = 0; i < 10; i++)
        {
            _tables.Add(new Table(i));
        }

        var provider = _notifyProvider = NotifyProvider.Create(new MessagingConfiguration());
        
        provider
            .ConfigureChannel(NotificationExchange)
            .ExchangeDeclare(exchange: NotificationExchange, type: ExchangeType.Fanout);
        
        FreeTables(_freeTablesCancellationSource.Token);
    }

    private async Task FreeTables(CancellationToken token)
    {
        while (await _timer.WaitForNextTickAsync(token))
        {
            if(token.IsCancellationRequested) return;
            
            var bookedTables = _tables.Where(t => t.State == TableState.Booked).Select(t => t.Id).ToArray();
            if(bookedTables.Length == 0) continue;

            foreach (var tableId in bookedTables)
                FreeTableAsync(tableId);
        }
    }

    public void BookFreeTable(int countOfPersons)
    {
        _event.WaitOne();
        Console.WriteLine("Добрый день! Подождите секунду я подберу столик и подтвержу вашу бронь, оставайтесь на линии");
        
        var table = _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);
        
        Thread.Sleep(1000*5);
        
        Console.WriteLine(table is null
            ? "К сожалению, сейчас все столики заняты"
            : "Готово! Ваш столик номер " + table.Id);
        _event.Set();
    }

    public void BookFreeTableAsync(int countOfPersons)
    {
        _event.WaitOne();
        Console.WriteLine("Добрый день! Подождите секунду я подберу столик и подтвержу вашу бронь, вам придёт уведомление");

        Task.Run(async () =>
        {
            
            var table = _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);

            await Task.Delay(1000 * 5);
            table?.Set(TableState.Booked);

            _notifyProvider.Send(
                NotificationExchange,
                string.Empty,
                table is null
                    ? "К сожалению, сейчас все столики заняты"
                    : "Готово! Ваш столик номер " + table.Id, 
                NotificationExchange);
            _event.Set();
        });
    }
    
    public void FreeTable(int id)
    {
        _event.WaitOne();
        Console.WriteLine("Добрый день! Подождите секунду я освобожу столик, оставайтесь на линии");
        var table = _tables.FirstOrDefault(t => t.Id == id);
        
        Thread.Sleep(1000*5);

        table?.Set(TableState.Free);

        Console.WriteLine(table is null
            ? "Такого столика нет в нашем ресторане"
            : "Готово! Мы отменили вашу бронь");
        _event.Set();
    }

    public void FreeTableAsync(int id)
    {
        _event.WaitOne();
        Console.WriteLine("Добрый день! Подождите секунду я подберу столик и подтвержу вашу бронь, вам придёт уведомление");

        Task.Run(async () =>
        {
            var table = _tables.FirstOrDefault(t => t.Id == id && t.State == TableState.Booked);

            await Task.Delay(1000 * 5);
            
            table?.Set(TableState.Free);

            _notifyProvider.Send(
                NotificationExchange,
                string.Empty,
                table is null
                    ? "Такого столика нет в нашем ресторане"
                    : "Готово! Мы отменили вашу бронь", 
                NotificationExchange);
            _event.Set();
        });
    }

    public void Dispose()
    {
        _freeTablesCancellationSource.Cancel();
        _event.Dispose();
        _timer.Dispose();
        _freeTablesCancellationSource.Dispose();
        _notifyProvider.Dispose();
    }
}