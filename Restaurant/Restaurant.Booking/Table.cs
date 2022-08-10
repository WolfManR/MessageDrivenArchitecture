namespace Restaurant.Booking;

/// <summary>
/// Table in restaurant
/// </summary>
public class Table
{
    private readonly object _locker = new();
    public TableState State { get; private set; }
    public int SeatsCount { get; }
    public int Id { get; }

    public Table(int id)
    {
        Id = id;
        State = TableState.Free;
        SeatsCount = Random.Shared.Next(2, 5);
    }

    /// <summary>
    /// Set table state
    /// </summary>
    /// <returns>true if state set, false if state of table same as value</returns>
    public bool Set(TableState state)
    {
        lock (_locker)
        {
            if (state == State) return false;

            State = state;
            return true;
        }
    }
}