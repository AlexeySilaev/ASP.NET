using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Collections.Concurrent;

namespace PromoCodeFactory.DataAccess.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ConcurrentDictionary<Guid, T> _data;

    public InMemoryRepository(IEnumerable<T> data)
    {
        _data = new ConcurrentDictionary<Guid, T>(data.Select(e => new KeyValuePair<Guid, T>(e.Id, e)));
    }
    public Task<IReadOnlyCollection<T>> GetAll(CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<T>)_data.Values);
    }

    public Task<T?> GetById(Guid id, CancellationToken ct)
    {
        _data.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task Add(T entity, CancellationToken ct)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if(!_data.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"Сушность с id = '{entity.Id}' уже существует");

        return Task.CompletedTask;
    }

    public Task Update(T entity, CancellationToken ct)
    {
        CheckExists(entity.Id);
        _data[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task Delete(Guid id, CancellationToken ct)
    {
        CheckExists(id);
        _data.Remove(id, out _);
        return Task.CompletedTask;
    }

    private void CheckExists(Guid id)
    {
        if (_data.ContainsKey(id) == false) // я привык явно сравнивать с false, это виднее, чем слепой оператор '!'
            throw new EntityNotFoundException(typeof(T), id);
    }
}
