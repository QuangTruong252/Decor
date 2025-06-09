using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Models;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DecorStore.API.Repositories
{    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly ICacheService _cacheService;
        
        public ProductRepository(ApplicationDbContext context, ICacheService cacheService) : base(context)
        {
            _cacheService = cacheService;
        }        public async Task<PagedResult<Product>> GetPagedAsync(ProductFilterDTO filter)
        {
            // Create cache key based on filter parameters
            var cacheKey = $"products:paged:{filter.GetHashCode()}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = GetFilteredProducts(filter);
                
                // Use separate optimized query for count to avoid loading unnecessary data
                var totalCountQuery = GetFilteredProductsForCount(filter);
                var totalCount = await totalCountQuery.CountAsync();
                
                var items = await query
                    .AsSplitQuery() // Optimize complex includes
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PagedResult<Product>(items, totalCount, filter.PageNumber, filter.PageSize);
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await GetFilteredProducts(filter)
                .AsSplitQuery() // Optimize complex includes
                .ToListAsync();
        }

        // Override base GetByIdAsync to include related data
        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await GetQueryableNoTracking()
                .AsSplitQuery() // Optimize multiple includes
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Reviews.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await GetQueryableNoTracking()
                .AsSplitQuery() // Optimize multiple includes
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Reviews.Where(r => !r.IsDeleted))
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<int> GetTotalCountAsync(ProductFilterDTO filter)
        {
            return await GetFilteredProductsForCount(filter).CountAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Slug == slug && !p.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug, int excludeProductId)
        {
            return await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Slug == slug && p.Id != excludeProductId && !p.IsDeleted);
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            return await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.SKU == sku && !p.IsDeleted);
        }

        public async Task<bool> SkuExistsAsync(string sku, int excludeProductId)
        {
            return await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.SKU == sku && p.Id != excludeProductId && !p.IsDeleted);
        }        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            var cacheKey = $"products:featured:{count}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _context.Products
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                    .Where(p => p.IsFeatured && p.IsActive && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }, TimeSpan.FromMinutes(30), CacheItemPriority.High, "products:featured");
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 20)
        {
            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            // Optimize by getting category in a single query instead of loading full product
            var categoryId = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => p.CategoryId)
                .FirstOrDefaultAsync();

            if (categoryId == 0) return new List<Product>();

            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.CategoryId == categoryId && p.Id != productId && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10)
        {
            var cacheKey = $"products:top-rated:{count}";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _context.Products
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                    .Where(p => p.IsActive && !p.IsDeleted && p.AverageRating > 0)
                    .OrderByDescending(p => p.AverageRating)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }, TimeSpan.FromMinutes(30), CacheItemPriority.High, "products:top-rated");
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.StockQuantity <= threshold && p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        private IQueryable<Product> GetFilteredProducts(ProductFilterDTO filter)
        {
            var query = _context.Products
                .AsNoTracking() // Performance optimization for read-only operations
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => !p.IsDeleted) // Always filter out deleted items
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    EF.Functions.Contains(p.Name, searchTerm) ||
                    EF.Functions.Contains(p.Description, searchTerm) ||
                    EF.Functions.Contains(p.SKU, searchTerm)
                );
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive);

            if (filter.CreatedAfter.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore);

            if (filter.StockQuantityMin.HasValue)
                query = query.Where(p => p.StockQuantity >= filter.StockQuantityMin);

            if (filter.StockQuantityMax.HasValue)
                query = query.Where(p => p.StockQuantity <= filter.StockQuantityMax);

            if (filter.MinRating.HasValue)
                query = query.Where(p => p.AverageRating >= filter.MinRating);

            if (!string.IsNullOrWhiteSpace(filter.SKU))
                query = query.Where(p => EF.Functions.Contains(p.SKU, filter.SKU));

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                "rating_asc" => query.OrderBy(p => p.AverageRating),
                "rating_desc" => query.OrderByDescending(p => p.AverageRating),
                "stock_asc" => query.OrderBy(p => p.StockQuantity),
                "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }

        /// <summary>
        /// Optimized query for count operations - excludes expensive includes
        /// </summary>
        private IQueryable<Product> GetFilteredProductsForCount(ProductFilterDTO filter)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    EF.Functions.Contains(p.Name, searchTerm) ||
                    EF.Functions.Contains(p.Description, searchTerm) ||
                    EF.Functions.Contains(p.SKU, searchTerm)
                );
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive);

            if (filter.CreatedAfter.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore);

            if (filter.StockQuantityMin.HasValue)
                query = query.Where(p => p.StockQuantity >= filter.StockQuantityMin);

            if (filter.StockQuantityMax.HasValue)
                query = query.Where(p => p.StockQuantity <= filter.StockQuantityMax);

            if (filter.MinRating.HasValue)
                query = query.Where(p => p.AverageRating >= filter.MinRating);

            if (!string.IsNullOrWhiteSpace(filter.SKU))
                query = query.Where(p => EF.Functions.Contains(p.SKU, filter.SKU));

            return query;
        }
    }
}
