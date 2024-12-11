using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class RepositoryBaseAsync<T> : IRepositoryBaseAsync<T> where T : EntityBase
    {
        protected readonly SmartFarmContext _dbContext;

        public RepositoryBaseAsync(SmartFarmContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));


        }

        public IQueryable<T> FindAll(bool trackChanges = false)
        {
            return !trackChanges ? _dbContext.Set<T>().AsNoTracking() :
                _dbContext.Set<T>();
        }
        public IQueryable<T> FindAll(bool trackChanges, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindAll(trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false)
        {
            return !trackChanges ? _dbContext.Set<T>().Where(expression).AsNoTracking()
              : _dbContext.Set<T>().Where(expression);
        }
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindByCondition(expression, trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await FindByCondition(T => T.Id.Equals(id)).FirstOrDefaultAsync();
        }
        public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includeProperties)
        {
            return await FindByCondition(T => T.Id.Equals(id), trackChanges: false, includeProperties).FirstOrDefaultAsync();
        }
        public Task<IDbContextTransaction> BeginTransactionAsync()
         => _dbContext.Database.BeginTransactionAsync();

        public async System.Threading.Tasks.Task EndTransactionAsync()
        {
            await _dbContext.SaveChangesAsync();
            await _dbContext.Database.CommitTransactionAsync();
        }
        public async System.Threading.Tasks.Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        public async Task<Guid> CreateAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity.Id;
        }
        public async Task<IList<Guid>> CreateListAsync(IEnumerable<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            return entities.Select(x => x.Id).ToList();
        }
        public System.Threading.Tasks.Task UpdateAsync(T entity)
        {
            if (_dbContext.Entry(entity).State == EntityState.Unchanged) return System.Threading.Tasks.Task.CompletedTask;
            T exist = _dbContext.Set<T>().Find(entity.Id);
            _dbContext.Entry(exist).CurrentValues.SetValues(entity);
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public async System.Threading.Tasks.Task UpdateListAsync(IEnumerable<T> entities)
        => await _dbContext.Set<T>().AddRangeAsync(entities);



        public System.Threading.Tasks.Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task DeleteListAsync(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }

        public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>>? filter = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    int page = 1,
    int pageSize = 10,
    params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
