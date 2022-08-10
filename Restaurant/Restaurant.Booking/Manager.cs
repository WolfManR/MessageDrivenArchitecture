﻿namespace Restaurant.Booking;

/// <summary>
/// Restaurant manager
/// </summary>
public class Manager
{
    private readonly Hall _hall;

    public Manager(Hall hall)
    {
        _hall = hall;
    }

    /// <summary>
    /// Books a table
    /// </summary>
    public async Task<bool> BookFreeTable(int countOfPersons)
    {
        await Task.Delay(1000 * 5);

        var table = _hall.FindFreeTable(countOfPersons);

        table?.Set(TableState.Booked);

        return table is not null;
        // Console.WriteLine(table is null
        //     ? "К сожалению, сейчас все столики заняты"
        //     : "Готово! Ваш столик номер " + table.Id);
    }

    /// <summary>
    /// Releases the reserved table (asynchronously)
    /// </summary>
    public async Task FreeTableAsync(int id)
    {
        var table = _hall.FindTable(id);

        await Task.Delay(1000 * 5);

        if (table is not null)
        {
            table.Set(TableState.Free);

            // _notifyProvider.Send(
            //     NotificationExchange,
            //     string.Empty, 
            //     "Столик под номером" + id + "свободен", 
            //     NotificationExchange);
        }
    }
}