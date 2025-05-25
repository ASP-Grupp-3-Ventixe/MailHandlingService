using System.Diagnostics;
using System.Linq.Expressions;
using Application.Interfaces;
using Application.Responses;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly MailDbContext _context;
    protected readonly DbSet<TEntity> _table;

    protected BaseRepository(MailDbContext context)
    {
        _context = context;
        _table = _context.Set<TEntity>();
    }
    
    // Add a new entity to the database
    public virtual async Task<RepositoryResult> AddAsync(TEntity? entity)
    {
        try
        {
            if (entity == null)
                return new RepositoryResult { Succeeded = false, StatusCode = 400, Error = "Invalid properties" };

            _table.Add(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult { Succeeded = true, StatusCode = 201 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }
    
    // Get all entities, with options for sorting, filtering, and including related data.
    public virtual async Task<RepositoryResult<IEnumerable<TEntity>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortByColumn = null, Expression<Func<TEntity, bool>>? filterBy = null, int take = 0, params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> query = _table;

        if (filterBy != null)
            query = query.Where(filterBy);

        if (includes != null && includes.Length != 0)
            foreach (var include in includes)
                query = query.Include(include);

        if (sortByColumn != null)
            query = orderByDescending
                ? query.OrderByDescending(sortByColumn)
                : query.OrderBy(sortByColumn);

        if (take > 0)
            query = query.Take(take);

        var entities = await query.ToListAsync();
        return new RepositoryResult<IEnumerable<TEntity>> { Succeeded = true, StatusCode = 200, Result = entities };
    }


    // Get a single entity that matches a condition, with optional related data.
    public virtual async Task<RepositoryResult<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? findBy, params Expression<Func<TEntity, object>>[]? includes)
    {
        IQueryable<TEntity> query = _table;

        if (findBy == null)
            return new RepositoryResult<TEntity> { Succeeded = false, StatusCode = 400, Error = "Expression not defined." };

        if (includes != null && includes.Length != 0)
            foreach (var include in includes)
                query = query.Include(include);

        var entity = await query.FirstOrDefaultAsync(findBy);
        return entity != null
            ? new RepositoryResult<TEntity> { Succeeded = true, StatusCode = 200, Result = entity }
            : new RepositoryResult<TEntity> { Succeeded = false, StatusCode = 404, Error = "Entity not found." };
    }


    public virtual async Task<RepositoryResult> ExistsAsync(Expression<Func<TEntity, bool>>? findBy)
    {
        if (findBy == null)
            return new RepositoryResult { Succeeded = false, StatusCode = 400, Error = "Invalid expression" };

        if (!await _table.AnyAsync(findBy))
            return new RepositoryResult { Succeeded = false, StatusCode = 404, Error = "Entity not found." };

        return new RepositoryResult { Succeeded = true, StatusCode = 200, Error = "Entity exists." };
    }
    

    public virtual async Task<RepositoryResult> UpdateAsync(TEntity? entity)
    {
        try
        {
            if (entity == null)
                return new RepositoryResult { Succeeded = false, StatusCode = 400, Error = "Invalid properties" };

            if (!await _table.ContainsAsync(entity))
                return new RepositoryResult { Succeeded = false, StatusCode = 404, Error = "Entity not found." };
            
            await _context.SaveChangesAsync();
            return new RepositoryResult { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult> DeleteAsync(Expression<Func<TEntity, bool>>? findBy)
    {
        try
        {
            if (findBy == null)
                return new RepositoryResult { Succeeded = false, StatusCode = 400, Error = "Invalid expression" };

            var entity = await _table.FirstOrDefaultAsync(findBy);
            if (entity == null)
                return new RepositoryResult { Succeeded = false, StatusCode = 404, Error = "Entity not found." };

            _table.Remove(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult> DeleteManyAsync(Expression<Func<TEntity, bool>>? filterBy)
    {
        try
        {
            if (filterBy == null)
                return new RepositoryResult { Succeeded = false, StatusCode = 400, Error = "Invalid expression" };

            var entities = await _table.Where(filterBy).ToListAsync();
            if (!entities.Any())
                return new RepositoryResult { Succeeded = false, StatusCode = 404, Error = "No entities found." };

            _table.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return new RepositoryResult { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }
}
