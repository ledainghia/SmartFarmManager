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
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includeProperties);

    }
    public interface IRepositoryBaseAsync<T> : IRepositoryQueryBase<T> where T : EntityBase
    {
        Task<int> CreateAsync(T entity);
        Task<IList<int>> CreateListAsync(IEnumerable<T> entities);
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

    }
}
