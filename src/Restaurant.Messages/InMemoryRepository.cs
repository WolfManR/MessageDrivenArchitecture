using System.Collections.Concurrent;

namespace Restaurant.Messages;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly ConcurrentBag<T> _repo = new ();

    public void AddOrUpdate(T entity)
    {
        _repo.Add(entity);
    }

    public IEnumerable<T> Get()
    {
        return _repo;
    }
}