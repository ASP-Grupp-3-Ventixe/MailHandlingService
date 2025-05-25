using System.Linq.Expressions;
using Application.Responses;

namespace Application.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<RepositoryResult> AddAsync(TEntity entity);
    Task<RepositoryResult<IEnumerable<TEntity>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortByColumn = null, Expression<Func<TEntity, bool>>? filterBy = null, int take = 0, params Expression<Func<TEntity, object>>[] includes);
    Task<RepositoryResult<TEntity>> GetAsync(Expression<Func<TEntity, bool>> findBy, params Expression<Func<TEntity, object>>[] includes);
    Task<RepositoryResult> ExistsAsync(Expression<Func<TEntity, bool>> findBy);
    Task<RepositoryResult> UpdateAsync(TEntity entity);
    Task<RepositoryResult> DeleteAsync(Expression<Func<TEntity, bool>> findBy);
    Task<RepositoryResult> DeleteManyAsync(Expression<Func<TEntity, bool>>? filterBy);
}