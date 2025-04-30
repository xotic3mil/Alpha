using Data.Contexts;
using Domain.Extensions;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Data.Repositories;

public abstract class BaseRepository<TEntity, Tmodel>(DataContext context) : IBaseRepository<TEntity, Tmodel> where TEntity : class
{
    protected readonly DataContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();


    public virtual async Task<RepositoryResult<bool>> CreateAsync(TEntity entity)
    {
        if (entity == null)
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Entity can't be null" };

        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult<bool> { Succeeded = true, StatusCode = 201 };
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Error Creating {nameof(TEntity)} :: {ex.Message}");
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };

        }
    }

    public virtual async Task<RepositoryResult<IEnumerable<Tmodel>>> GetAllAsync(
        bool orderbyDescending = false,
        Expression<Func<TEntity, object>>? expression = null,
        Expression<Func<TEntity, bool>>? where = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (where != null)
            query = query.Where(where);

        if (includes != null && includes.Length != 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (expression != null)
            query = orderbyDescending
                ? query.OrderByDescending(expression)
                : query.OrderBy(expression);

        var entities = await query.ToListAsync();
        var result = entities.Select(entity => entity.MapTo<Tmodel>());
        return new RepositoryResult<IEnumerable<Tmodel>> { Succeeded = true, StatusCode = 200, Result = result };
    }


    public virtual async Task<RepositoryResult<IEnumerable<TSelect>>> GetAllAsync<TSelect>(Expression<Func<TEntity, TSelect>> selector, bool orderbyDescending = false, Expression<Func<TEntity, object>>? expression = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes)
    {

        IQueryable<TEntity> query = _dbSet;
        if (where != null)
            query = query.Where(where);

        if (includes != null && includes.Length != 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (expression != null)
            query = orderbyDescending
                ? query.OrderByDescending(expression)
                : query.OrderBy(expression);


        var entities = await query.Select(selector).ToListAsync();
        var result = entities.Select(entities => entities!.MapTo<TSelect>());
        return new RepositoryResult<IEnumerable<TSelect>> { Succeeded = true, StatusCode = 200, Result = result };
    }

    public async Task<RepositoryResult<Tmodel>> GetAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes)
    {

        IQueryable<TEntity> query = _dbSet;

        if (includes != null && includes.Length != 0)
            foreach (var include in includes)
                query = query.Include(include);

        var entities = await query.AsNoTracking().FirstOrDefaultAsync(where);
        if (entities == null)
            return new RepositoryResult<Tmodel> { Succeeded = false, StatusCode = 404, Error = "not found" };


        var result = entities.MapTo<Tmodel>();
        return new RepositoryResult<Tmodel> { Succeeded = true, StatusCode = 200, Result = result };
    }

    public virtual async Task<RepositoryResult<bool>> ExistsAsync(Expression<Func<TEntity, bool>> expression)
    {
        var exists = await _dbSet.AnyAsync(expression);
        return !exists
            ? new RepositoryResult<bool> { Succeeded = false, StatusCode = 404, Error = "not found" }
            : new RepositoryResult<bool> { Succeeded = true, StatusCode = 200 };
    }

    public virtual async Task<RepositoryResult<bool>> UpdateAsync<TUpdateModel>(TUpdateModel model, Expression<Func<TEntity, bool>> expression) where TUpdateModel : class
    {
        if (model == null)
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Model can't be null" };

        try
        {
            var existingEntity = await _dbSet.FirstOrDefaultAsync(expression);
            if (existingEntity == null)
                return new RepositoryResult<bool> { Succeeded = false, StatusCode = 404, Error = "Entity not found" };

            var updatedEntity = model.MapTo<TEntity>();

            foreach (var property in typeof(TEntity).GetProperties().Where(p => p.CanWrite))
            {
                var value = property.GetValue(updatedEntity);
                if (value != null)
                {
                    property.SetValue(existingEntity, value);
                }
            }

            _context.Entry(existingEntity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return new RepositoryResult<bool> { Succeeded = true, StatusCode = 200, Result = true };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Debug.WriteLine($"Concurrency error updating {nameof(TEntity)} :: {ex.Message}");
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 409, Error = "The record was modified by another user. Please refresh and try again." };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating {nameof(TEntity)} :: {ex.Message}");
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }


    public virtual async Task<RepositoryResult<bool>> DeleteAsync(TEntity entity)
    {
        if(entity == null)
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Entity can't be null" };

        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult<bool> { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error Deleting {nameof(TEntity)} :: {ex.Message}");
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

}
