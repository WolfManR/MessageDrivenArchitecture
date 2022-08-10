namespace Restourant;

/// <summary>
/// Table in restaurant
/// </summary>
public class Table
{
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
        if (state == State) return false;

        State = state;
        return true;
    }
}