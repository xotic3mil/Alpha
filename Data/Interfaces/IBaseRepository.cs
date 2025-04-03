using Data.Entities;
using Data.Models;
using System.Linq.Expressions;

namespace Data.Interfaces;

public interface IBaseRepository<TEntity, Tmodel> where TEntity : class
{

    public Task<RepositoryResult<bool>> CreateAsync(TEntity entity);
    public Task<RepositoryResult<bool>> DeleteAsync(TEntity entity);
    public Task<RepositoryResult<IEnumerable<Tmodel>>> GetAllAsync(bool orderbyDescending = false, Expression<Func<TEntity, object>>? expression = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes);
    public Task<RepositoryResult<IEnumerable<TSelect>>> GetAllAsync<TSelect>(Expression<Func<TEntity, TSelect>> selector, bool orderbyDescending = false, Expression<Func<TEntity, object>>? expression = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes);
    public Task<RepositoryResult<Tmodel>> GetAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes);
    public Task<RepositoryResult<bool>> ExistsAsync(Expression<Func<TEntity, bool>> expression);
}
