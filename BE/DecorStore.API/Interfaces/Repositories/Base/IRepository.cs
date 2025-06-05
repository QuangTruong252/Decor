using System.Linq.Expressions;
using DecorStore.API.DTOs;

namespace DecorStore.API.Interfaces.Repositories.Base
{
    /// <summary>
    /// Generic repository interface for common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {        // Basic CRUD Operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // Advanced Query Operations
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Pagination Support
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate);

        // Bulk Operations
        Task BulkDeleteAsync(IEnumerable<int> ids);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Specification Pattern Support (for complex queries)
        Task<IEnumerable<T>> GetWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdWithIncludeAsync(int id, params Expression<Func<T, object>>[] includes);
    }
}
