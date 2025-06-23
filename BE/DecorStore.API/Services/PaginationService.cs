using Microsoft.EntityFrameworkCore;
using DecorStore.API.DTOs;
using System.Linq.Expressions;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for optimized pagination operations
    /// </summary>
    public interface IPaginationService
    {
        Task<PagedResult<T>> GetPagedResultAsync<T>(IQueryable<T> query, PaginationParameters parameters) where T : class;
        Task<PagedResult<T>> GetPagedResultAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class;
        Task<PagedResult<TResult>> GetPagedResultAsync<TSource, TResult>(
            IQueryable<TSource> query, 
            Expression<Func<TSource, TResult>> selector,
            PaginationParameters parameters) where TSource : class where TResult : class;
        Task<int> GetOptimizedCountAsync<T>(IQueryable<T> query);
    }

    /// <summary>
    /// Optimized pagination service implementation
    /// </summary>
    public class PaginationService : IPaginationService
    {
        private readonly ILogger<PaginationService> _logger;

        public PaginationService(ILogger<PaginationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets paginated results with optimized counting
        /// </summary>
        public async Task<PagedResult<T>> GetPagedResultAsync<T>(IQueryable<T> query, PaginationParameters parameters) where T : class
        {
            return await GetPagedResultAsync(query, parameters.PageNumber, parameters.PageSize);
        }

        /// <summary>
        /// Gets paginated results with basic parameters
        /// </summary>
        public async Task<PagedResult<T>> GetPagedResultAsync<T>(IQueryable<T> query, int pageNumber, int pageSize) where T : class
        {
            var totalCount = await GetOptimizedCountAsync(query);
            
            if (totalCount == 0)
            {
                return new PagedResult<T>(new List<T>(), 0, pageNumber, pageSize);
            }

            var skip = (pageNumber - 1) * pageSize;
            
            // Optimize skip for large offsets using cursor-based pagination where possible
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking() // Performance optimization for read-only operations
                .ToListAsync();

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Gets paginated results with projection for better performance
        /// </summary>
        public async Task<PagedResult<TResult>> GetPagedResultAsync<TSource, TResult>(
            IQueryable<TSource> query, 
            Expression<Func<TSource, TResult>> selector,
            PaginationParameters parameters) where TSource : class where TResult : class
        {
            var totalCount = await GetOptimizedCountAsync(query);
            
            if (totalCount == 0)
            {
                return new PagedResult<TResult>(new List<TResult>(), 0, parameters.PageNumber, parameters.PageSize);
            }

            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            
            var items = await query
                .Skip(skip)
                .Take(parameters.PageSize)
                .Select(selector) // Project early to reduce data transfer
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<TResult>(items, totalCount, parameters.PageNumber, parameters.PageSize);
        }

        /// <summary>
        /// Optimized count operation with caching for large datasets
        /// </summary>
        public async Task<int> GetOptimizedCountAsync<T>(IQueryable<T> query)
        {
            try
            {
                // For very large datasets, we might want to estimate or cache counts
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting query results");
                throw;
            }
        }
    }

    /// <summary>
    /// Cursor-based pagination for very large datasets
    /// </summary>
    public interface ICursorPaginationService
    {
        Task<CursorPagedResult<T>> GetCursorPagedResultAsync<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            TKey? cursor,
            int pageSize,
            bool ascending = true) where TKey : IComparable<TKey> where T : class;
    }

    /// <summary>
    /// Cursor-based paginated result
    /// </summary>
    public class CursorPagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public string? NextCursor { get; set; }
        public string? PreviousCursor { get; set; }
        public bool HasMore { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Cursor-based pagination service for optimal performance on large datasets
    /// </summary>
    public class CursorPaginationService : ICursorPaginationService
    {
        private readonly ILogger<CursorPaginationService> _logger;

        public CursorPaginationService(ILogger<CursorPaginationService> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Gets cursor-based paginated results - more efficient for large datasets
        /// </summary>
        public async Task<CursorPagedResult<T>> GetCursorPagedResultAsync<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            TKey? cursor,
            int pageSize,
            bool ascending = true) where TKey : IComparable<TKey> where T : class
        {
            try
            {
                // Apply cursor filtering
                if (cursor != null)
                {
                    if (ascending)
                    {
                        var parameter = keySelector.Parameters[0];
                        var property = ((MemberExpression)keySelector.Body).Member.Name;
                        query = query.Where(BuildComparisonExpression<T, TKey>(property, cursor, ">"));
                    }
                    else
                    {
                        var parameter = keySelector.Parameters[0];
                        var property = ((MemberExpression)keySelector.Body).Member.Name;
                        query = query.Where(BuildComparisonExpression<T, TKey>(property, cursor, "<"));
                    }
                }

                // Order the query
                query = ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);

                // Get one extra item to determine if there are more results
                var items = await query
                    .Take(pageSize + 1)
                    .AsNoTracking()
                    .ToListAsync();

                var hasMore = items.Count > pageSize;
                var actualItems = hasMore ? items.Take(pageSize).ToList() : items;

                // Extract cursors
                string? nextCursor = null;
                if (hasMore && actualItems.Any())
                {
                    var lastItem = actualItems.Last();
                    var lastKey = keySelector.Compile()(lastItem);
                    nextCursor = Convert.ToString(lastKey);
                }

                return new CursorPagedResult<T>
                {
                    Items = actualItems,
                    NextCursor = nextCursor,
                    HasMore = hasMore,
                    Count = actualItems.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cursor-based pagination");
                throw;
            }
        }

        private static Expression<Func<T, bool>> BuildComparisonExpression<T, TKey>(
            string propertyName, 
            TKey value, 
            string operation) where TKey : IComparable<TKey>
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var constant = Expression.Constant(value);
            
            BinaryExpression comparison = operation switch
            {
                ">" => Expression.GreaterThan(property, constant),
                "<" => Expression.LessThan(property, constant),
                ">=" => Expression.GreaterThanOrEqual(property, constant),
                "<=" => Expression.LessThanOrEqual(property, constant),
                _ => throw new ArgumentException($"Unsupported operation: {operation}")
            };

            return Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }
    }
}
