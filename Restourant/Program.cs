using System.Diagnostics;
using System.Text;
using ConsoleMenu;

Console.OutputEncoding = Encoding.UTF8;
using var rest = new Hall();

Menu bookTableMenu = new();
bookTableMenu.SubTitle = "выберите способ уведомления";
bookTableMenu.AddMenuItem("мы уведомим Вас по смс (асинхронно)", BookTable(rest.BookFreeTableAsync).LogExecuteTime);
bookTableMenu.AddMenuItem("подождите на линии, мы Вас оповестим (синхронно)", BookTable(rest.BookFreeTable).LogExecuteTime);
bookTableMenu.AddMenuItem("обратно", bookTableMenu.Back);

Menu freeTableMenu = new();
freeTableMenu.SubTitle = "выберите способ уведомления";
freeTableMenu.AddMenuItem("мы уведомим Вас по смс (асинхронно)", FreeTable(rest.FreeTableAsync).LogExecuteTime);
freeTableMenu.AddMenuItem("подождите на линии, мы Вас оповестим (синхронно)", FreeTable(rest.FreeTable).LogExecuteTime);
freeTableMenu.AddMenuItem("обратно", freeTableMenu.Back);
freeTableMenu.AddMenuItem("Выход", freeTableMenu.Close);

Menu mainMenu = new();
mainMenu.SubTitle = "Привет! Желаете забронировать или освободить столик?";
bookTableMenu.ParentMenu = mainMenu;
freeTableMenu.ParentMenu = mainMenu;

mainMenu.AddMenuItem("Забронировать", bookTableMenu.ShowMenu);
mainMenu.AddMenuItem("Освободить", freeTableMenu.ShowMenu);
mainMenu.AddMenuItem("Выход", mainMenu.Close);
mainMenu.ShowMenu();

Action BookTable(Action<int> bookBehavior)
{
    return () =>
    {
        Console.WriteLine("На сколько мест столик желаете:");
        int.TryParse(Console.ReadLine(), out var places);
        bookBehavior.Invoke(places);
    };
}

Action FreeTable(Action<int> freeBehavior)
{
    return () =>
    {
        Console.WriteLine("Укажите номер столика");
        int.TryParse(Console.ReadLine(), out var tableId);
        freeBehavior.Invoke(tableId);
    };
}

static class LogExtension
{
    public static void LogExecuteTime(this Action execution)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
    
        execution.Invoke();
    
        stopWatch.Stop();
        var ts = stopWatch.Elapsed;
        Console.WriteLine($"{ts.Seconds:80}:{ts.Milliseconds:00}");
    }
}
