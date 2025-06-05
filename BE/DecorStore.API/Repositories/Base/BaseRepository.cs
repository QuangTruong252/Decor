using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Repositories.Base;

namespace DecorStore.API.Repositories.Base
{
    /// <summary>
    /// Generic base repository implementation for common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type that implements IBaseEntity</typeparam>
    public class BaseRepository<T> : IRepository<T> where T : class, IBaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        #region Basic CRUD Operations

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;

            var result = await _dbSet.AddAsync(entity);
            return result.Entity;
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;

            var result = await _dbSet.AddAsync(entity);
            return result.Entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            
            // Ensure CreatedAt and IsDeleted are not modified during updates
            _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
            _context.Entry(entity).Property(e => e.IsDeleted).IsModified = false;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                // Soft delete
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(entity);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        #endregion

        #region Advanced Query Operations

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                .Where(e => !e.IsDeleted)
                .Where(predicate)
                .ToListAsync();
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync(e => !e.IsDeleted);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted)
                .CountAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted)
                .AnyAsync(predicate);
        }

        #endregion

        #region Pagination Support

        public virtual async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.AsNoTracking().Where(e => !e.IsDeleted);
            return await GetPagedResultAsync(query, pageNumber, pageSize);
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.AsNoTracking()
                .Where(e => !e.IsDeleted)
                .Where(predicate);
            return await GetPagedResultAsync(query, pageNumber, pageSize);
        }

        private async Task<PagedResult<T>> GetPagedResultAsync(IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        #endregion

        #region Bulk Operations

        public virtual async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var entities = await _dbSet
                .Where(e => ids.Contains(e.Id) && !e.IsDeleted)
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(entities);
        }

        #endregion

        #region Include Operations

        public virtual async Task<IEnumerable<T>> GetWithIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsNoTracking().Where(e => !e.IsDeleted);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetByIdWithIncludeAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsNoTracking().Where(e => e.Id == id && !e.IsDeleted);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Protected Helper Methods

        /// <summary>
        /// Gets a queryable for custom repository implementations
        /// </summary>
        protected IQueryable<T> GetQueryable()
        {
            return _dbSet.Where(e => !e.IsDeleted);
        }

        /// <summary>
        /// Gets a queryable with tracking for custom repository implementations
        /// </summary>
        protected IQueryable<T> GetQueryableWithTracking()
        {
            return _dbSet.Where(e => !e.IsDeleted);
        }

        /// <summary>
        /// Gets a no-tracking queryable for custom repository implementations
        /// </summary>
        protected IQueryable<T> GetQueryableNoTracking()
        {
            return _dbSet.AsNoTracking().Where(e => !e.IsDeleted);
        }

        #endregion
    }
}
