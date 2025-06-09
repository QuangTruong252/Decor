using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    /// <summary>
    /// Base class for pagination parameters
    /// </summary>
    public class PaginationParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        /// <summary>
        /// Sort field name
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc)
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Calculate skip count for database queries
        /// </summary>
        public int Skip => (PageNumber - 1) * PageSize;

        /// <summary>
        /// Validate sort direction
        /// </summary>
        public bool IsDescending => SortDirection?.ToLower() == "desc";
    }

    /// <summary>
    /// Generic wrapper for paginated results
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new PaginationMetadata();

        public PagedResult()
        {
        }

        public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            Pagination = new PaginationMetadata
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasNext = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPrevious = pageNumber > 1
            };
        }
    }

    /// <summary>
    /// Pagination metadata for API responses
    /// </summary>
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public int? NextPage => HasNext ? CurrentPage + 1 : null;
        public int? PreviousPage => HasPrevious ? CurrentPage - 1 : null;
    }

    /// <summary>
    /// Search parameters for text-based searching
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// Search term
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Search fields to include (if null, searches all searchable fields)
        /// </summary>
        public string[]? SearchFields { get; set; }

        /// <summary>
        /// Whether search should be case sensitive
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Whether to use exact match or partial match
        /// </summary>
        public bool ExactMatch { get; set; } = false;

        /// <summary>
        /// Check if search term is provided
        /// </summary>
        public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);
    }

    /// <summary>
    /// Date range filter parameters
    /// </summary>
    public class DateRangeParameters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Validate date range
        /// </summary>
        public bool IsValid => StartDate == null || EndDate == null || StartDate <= EndDate;

        /// <summary>
        /// Check if date range is provided
        /// </summary>
        public bool HasDateRange => StartDate.HasValue || EndDate.HasValue;
    }

    /// <summary>
    /// Numeric range filter parameters
    /// </summary>
    /// <typeparam name="T">Numeric type</typeparam>
    public class NumericRangeParameters<T> where T : struct, IComparable<T>
    {
        public T? MinValue { get; set; }
        public T? MaxValue { get; set; }

        /// <summary>
        /// Validate numeric range
        /// </summary>
        public bool IsValid => MinValue == null || MaxValue == null || MinValue.Value.CompareTo(MaxValue.Value) <= 0;

        /// <summary>
        /// Check if numeric range is provided
        /// </summary>
        public bool HasRange => MinValue.HasValue || MaxValue.HasValue;
    }

    /// <summary>
    /// Cursor-based pagination parameters for large datasets
    /// </summary>
    public class CursorPaginationParameters
    {
        /// <summary>
        /// Number of items to return
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int Take { get; set; } = 25;

        /// <summary>
        /// Cursor value for pagination (typically an ID or timestamp)
        /// </summary>
        public string? Cursor { get; set; }

        /// <summary>
        /// Sort field for cursor pagination
        /// </summary>
        public string SortBy { get; set; } = "Id";

        /// <summary>
        /// Sort direction for cursor pagination
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Whether to paginate forward or backward
        /// </summary>
        public bool IsDescending => SortDirection?.ToLower() == "desc";
    }

    /// <summary>
    /// Cursor-based paginated result for large datasets
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class CursorPagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public CursorPaginationMetadata Pagination { get; set; } = new CursorPaginationMetadata();

        public CursorPagedResult()
        {
        }

        public CursorPagedResult(IEnumerable<T> items, string? nextCursor, string? previousCursor, int requestedCount, bool hasMore)
        {
            Items = items;
            Pagination = new CursorPaginationMetadata
            {
                NextCursor = nextCursor,
                PreviousCursor = previousCursor,
                RequestedCount = requestedCount,
                ReturnedCount = items.Count(),
                HasNext = hasMore,
                HasPrevious = !string.IsNullOrEmpty(previousCursor)
            };
        }
    }

    /// <summary>
    /// Cursor pagination metadata
    /// </summary>
    public class CursorPaginationMetadata
    {
        public string? NextCursor { get; set; }
        public string? PreviousCursor { get; set; }
        public int RequestedCount { get; set; }
        public int ReturnedCount { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    /// <summary>
    /// Optimized pagination parameters with count optimization
    /// </summary>
    public class OptimizedPaginationParameters : PaginationParameters
    {
        /// <summary>
        /// Whether to skip total count calculation for performance
        /// </summary>
        public bool SkipTotalCount { get; set; } = false;

        /// <summary>
        /// Use estimated count for large datasets (faster but approximate)
        /// </summary>
        public bool UseEstimatedCount { get; set; } = false;

        /// <summary>
        /// Cache count result for subsequent requests
        /// </summary>
        public bool CacheCount { get; set; } = true;

        /// <summary>
        /// Count cache duration in seconds
        /// </summary>
        public int CountCacheDurationSeconds { get; set; } = 300; // 5 minutes
    }

    /// <summary>
    /// Optimized paginated result with performance enhancements
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class OptimizedPagedResult<T> : PagedResult<T>
    {
        /// <summary>
        /// Whether the total count is estimated
        /// </summary>
        public bool IsEstimatedCount { get; set; }

        /// <summary>
        /// Whether the count was retrieved from cache
        /// </summary>
        public bool IsCountFromCache { get; set; }

        /// <summary>
        /// Query execution time in milliseconds
        /// </summary>
        public long QueryTimeMs { get; set; }

        public OptimizedPagedResult()
        {
        }

        public OptimizedPagedResult(
            IEnumerable<T> items, 
            int totalCount, 
            int pageNumber, 
            int pageSize,
            bool isEstimatedCount = false,
            bool isCountFromCache = false,
            long queryTimeMs = 0) : base(items, totalCount, pageNumber, pageSize)
        {
            IsEstimatedCount = isEstimatedCount;
            IsCountFromCache = isCountFromCache;
            QueryTimeMs = queryTimeMs;
        }
    }
}
