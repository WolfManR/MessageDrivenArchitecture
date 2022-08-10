namespace Restaurant.Booking;

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

    public Table? FindFreeTable(int countOfPersons)
    {
        return _tables.FirstOrDefault(t => t.SeatsCount >= countOfPersons && t.State == TableState.Free);
    }

    public Table? FindTable(int id)
    {
        return _tables.FirstOrDefault(t => t.Id == id);
    }
}