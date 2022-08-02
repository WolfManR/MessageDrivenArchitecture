using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var rest = new Hall();
while (true)
{
    Console.WriteLine("Привет! Желаете забронировать столик?\n" +
                      "1 - мы уведомим Вас по смс (асинхронно)\n" +
                      "2 - подождите на линии, мы Вас оповестим (синхронно)");

    var choiceValid = int.TryParse(Console.ReadLine(), out var choice);
    if (!choiceValid || (choiceValid && choice is not (1 or 2)))
    {
        Console.WriteLine("Введите, пожалуйста 1 или 2");
        continue;
    }

    var stopWatch = new Stopwatch();
    stopWatch.Start();

    if (choice == 1)
    {
        rest.BookFreeTableAsync(1);
    }
    else
    {
        rest.BookFreeTable(1);
    }

    Console.WriteLine("Спасибо за Ваше обращение!");
    stopWatch.Stop();
    var ts = stopWatch.Elapsed;
    Console.WriteLine($"{ts.Seconds:80}:{ts.Milliseconds:00}");
}