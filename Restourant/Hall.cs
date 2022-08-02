public class Hall
{
    private readonly List<Table> _tables = new();

    public Hall()
    {
        for (byte i = 0; i < 10; i++)
        {
            _tables.Add(new Table(i));
        }
    }

    public void BookFreeTable(int countOfPersons)
    {
        Console.WriteLine("Добрый день! Подождите секунду я подберу столик и подтвержу вашу бронь, оставайтесь на линии");

        var table = _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);
        
        Thread.Sleep(1000*5);

        Console.WriteLine(table is null
            ? "К сожалению, сейчас все столики заняты"
            : "Готово! Ваш столик номер " + table.Id);
    }

    public void BookFreeTableAsync(int countOfPersons)
    {
        Console.WriteLine("Добрый день! Подождите секунду я подберу столик и подтвержу вашу бронь, вам придёт уведомление");

        Task.Run(async () =>
        {
            var table = _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);

            await Task.Delay(1000 * 5);
            table?.Set(TableState.Booked);

            Console.WriteLine("УВЕДОМЛЕНИЕ: " + table is null
                ? "К сожалению, сейчас все столики заняты"
                : "Готово! Ваш столик номер " + table.Id);
        });
    }
}