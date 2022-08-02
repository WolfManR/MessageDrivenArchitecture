using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
var rest = new Hall();
while (true)
{
    Console.WriteLine("Привет! Желаете забронировать или освободить столик?\n" +
                      "1 - забронировать, мы уведомим Вас по смс (асинхронно)\n" +
                      "2 - забронировать, подождите на линии, мы Вас оповестим (синхронно)\n"+
                      "3 - освободить, мы уведомим Вас по смс (асинхронно)\n" +
                      "4 - освободить, подождите на линии, мы Вас оповестим (синхронно)\n"
                      );

    var choiceValid = int.TryParse(Console.ReadLine(), out var choice);
    if (!choiceValid || (choiceValid && choice is < 1 or >4))
    {
        Console.WriteLine("Введите, пожалуйста от 1 до 4");
        continue;
    }

    var stopWatch = new Stopwatch();
    stopWatch.Start();

    switch (choice)
    {
        case 1:
            rest.BookFreeTableAsync(1);
            break;
        case 2:
            rest.BookFreeTable(1);
            break;
        case 3:
        case 4:
            Console.WriteLine("Укажите номер столика");
            int.TryParse(Console.ReadLine(), out var tableId);
            if (choice == 3)
            {
                rest.FreeTableAsync(tableId);
            }
            else
            {
                rest.FreeTable(tableId);
            }
            break;
    }

    Console.WriteLine("Спасибо за Ваше обращение!");
    stopWatch.Stop();
    var ts = stopWatch.Elapsed;
    Console.WriteLine($"{ts.Seconds:80}:{ts.Milliseconds:00}");
}