using MongoDB.Driver;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pcf.Administration.DataAccess.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;
        public MongoRepository(MongoUrl mongoUrl)
        {
            var dbClient = new MongoClient(mongoUrl);
            var db = dbClient.GetDatabase(mongoUrl.DatabaseName);
            _collection = db.GetCollection<T>($"{typeof(T).Name.ToLower()}s");
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        }

        public async Task DeleteAsync(T entity)
        {
            await _collection.DeleteOneAsync(x => x.Id == entity.Id);
        }

        public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids)
        {
            var entities = await _collection.Find(x => ids.Contains(x.Id)).ToListAsync();
            return entities;
        }

        public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).ToListAsync();
        }

    }
}
