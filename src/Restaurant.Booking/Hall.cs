namespace Restaurant.Booking;

/// <summary>
/// It is a projection of the restaurant hall
/// </summary>
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

    /// <summary>
    /// Search for free table in hall on <paramref name="countOfPersons"/>
    /// </summary>
    /// <returns>Table that now free, or null if there no free table on <paramref name="countOfPersons"/></returns>
    public Table? FindFreeTable(int countOfPersons)
    {
        return _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);
    }

    /// <summary>
    /// Search for table with it id
    /// </summary>
    /// <param name="id">Table id</param>
    /// <returns>Table that has same <paramref name="id"/></returns>
    public Table? FindTable(int id)
    {
        return _tables.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Table> ListTables() => _tables;
}