using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using System.Linq.Expressions;

namespace PromoCodeFactory.DataAccess.Repositories;

internal class EfRepository<T>(PromoCodeFactoryDbContext context) : IRepository<T> where T : BaseEntity
{
    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

    public async Task Add(T entity, CancellationToken ct)
    {
        await context.Set<T>().AddAsync(entity, ct);
        await context.SaveChangesAsync();
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        T? entity = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyCollection<T>> GetAll(bool withIncludes, CancellationToken ct)
    {
        return await context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetById(Guid id, bool withIncludes, CancellationToken ct)
    {
        return await context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<IReadOnlyCollection<T>> GetByRangeId(IEnumerable<Guid> ids, bool withIncludes = false, CancellationToken ct = default)
    {
        var result = context.Set<T>()
            .Where(e => ids.Contains(e.Id))
            .ToList()
            .AsReadOnly();
        return Task.FromResult((IReadOnlyCollection<T>)result);
    }

    public Task<IReadOnlyCollection<T>> GetWhere(Expression<Func<T, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
    {
        var result = context.Set<T>()
            .Where(predicate)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<T>>(result);
    }

    public async Task Update(T entity, CancellationToken ct)
    {
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
    }

}
