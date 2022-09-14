namespace Restaurant.Messages;

public interface IRepository<T> where T : class
{
    void AddOrUpdate(T entity);
    IEnumerable<T> Get();
}