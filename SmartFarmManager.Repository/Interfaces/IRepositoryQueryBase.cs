using Microsoft.EntityFrameworkCore.Storage;
using SmartFarmManager.DataAccessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Interfaces
{
    public interface IRepositoryQueryBase<T> where T : EntityBase
    {
        IQueryable<T> FindAll(bool trackChanges = false);
        IQueryable<T> FindAll(bool trackChanges, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includeProperties);

    }
    public interface IRepositoryBaseAsync<T> : IRepositoryQueryBase<T> where T : EntityBase
    {
        Task<Guid> CreateAsync(T entity);
        Task<IList<Guid>> CreateListAsync(IEnumerable<T> entities);
        System.Threading.Tasks.Task UpdateAsync(T entity);
        System.Threading.Tasks.Task UpdateListAsync(IEnumerable<T> entities);

        System.Threading.Tasks.Task DeleteAsync(T entity);
        System.Threading.Tasks.Task DeleteListAsync(IEnumerable<T> entities);

        Task<IDbContextTransaction> BeginTransactionAsync();
        System.Threading.Tasks.Task EndTransactionAsync();
        System.Threading.Tasks.Task RollbackTransactionAsync();
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int page = 1,
        int pageSize = 10,
        params Expression<Func<T, object>>[] includeProperties);
    }
}
