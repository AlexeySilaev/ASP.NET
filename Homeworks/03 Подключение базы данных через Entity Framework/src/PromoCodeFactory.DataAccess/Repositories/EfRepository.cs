using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Linq.Expressions;

namespace PromoCodeFactory.DataAccess.Repositories;

internal class EfRepository<T>(PromoCodeFactoryDbContext context) : IRepository<T> where T : BaseEntity
{
    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

    public async Task Add(T entity, CancellationToken ct)
    {
        await context.Set<T>().AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        T? entity = await context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            throw new EntityNotFoundException(typeof(T), id);

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<T>> GetAll(bool withIncludes, CancellationToken ct)
    {
        IQueryable<T> result = context.Set<T>();
        if (withIncludes)
            result = ApplyIncludes(result);
        return await result.ToListAsync(ct);
    }

    public async Task<T?> GetById(Guid id, bool withIncludes, CancellationToken ct)
    {
        IQueryable<T> result = context.Set<T>();
        if (withIncludes)
            result = ApplyIncludes(result);
        return await result.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<T>> GetByRangeId(IEnumerable<Guid> ids, bool withIncludes = false, CancellationToken ct = default)
    {
        var where = context.Set<T>()
            .Where(e => ids.Contains(e.Id));
        if (withIncludes)
            where = ApplyIncludes(where);

        return await where.ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<T>> GetWhere(Expression<Func<T, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
    {
        var where = context.Set<T>()
            .Where(predicate);
        if (withIncludes)
            where = ApplyIncludes(where);

        return await where.ToListAsync(ct);
    }

    public async Task Update(T entity, CancellationToken ct)
    {
        if (context.Set<T>().FirstOrDefault(x => x.Id == entity.Id) == null)
            throw new EntityNotFoundException(typeof(T), entity.Id);

        context.Set<T>().Update(entity);
        await context.SaveChangesAsync(ct);
    }

}
