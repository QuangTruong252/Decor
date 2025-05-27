using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using DecorStore.API.DTOs;
using System;

namespace DecorStore.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Product>> GetPagedAsync(ProductFilterDTO filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var query = BuildProductQuery(filter);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var items = await query
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var query = BuildProductQuery(filter);

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            return await query
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        private IQueryable<Product> BuildProductQuery(ProductFilterDTO filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Apply base filters
            query = query.Where(p => !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                // check encode escape character eg: Bistro%20Set => Bistro Set
                searchTerm = Uri.UnescapeDataString(searchTerm);

                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Slug.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm));
            }

            // Apply category filter
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Apply price range filters
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Apply featured filter
            if (filter.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
            }

            // Apply active filter
            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }

            // Apply date range filters
            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter.Value);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore.Value);
            }

            // Apply stock quantity filters
            if (filter.StockQuantityMin.HasValue)
            {
                query = query.Where(p => p.StockQuantity >= filter.StockQuantityMin.Value);
            }

            if (filter.StockQuantityMax.HasValue)
            {
                query = query.Where(p => p.StockQuantity <= filter.StockQuantityMax.Value);
            }

            // Apply rating filter
            if (filter.MinRating.HasValue)
            {
                query = query.Where(p => p.AverageRating >= filter.MinRating.Value);
            }

            // Apply SKU filter
            if (!string.IsNullOrEmpty(filter.SKU))
            {
                query = query.Where(p => p.SKU.ToLower().Contains(filter.SKU.ToLower()));
            }

            return query;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, PaginationParameters filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
            {
                return query.OrderByDescending(p => p.CreatedAt);
            }

            return filter.SortBy.ToLower() switch
            {
                "name" => filter.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => filter.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "createdat" => filter.IsDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                "updatedat" => filter.IsDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
                "stockquantity" => filter.IsDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                "averagerating" => filter.IsDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
                "category" => filter.IsDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }

        public async Task<int> GetTotalCountAsync(ProductFilterDTO filter)
        {
            var query = BuildProductQuery(filter);
            return await query.CountAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Products.AnyAsync(p => p.Slug == slug);
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            return await _context.Products.AnyAsync(p => p.SKU == sku);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = System.DateTime.UtcNow;
            _context.Products.Update(product);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.UpdatedAt = System.DateTime.UtcNow;
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsDeleted = true;
                product.UpdatedAt = System.DateTime.UtcNow;
            }
        }

        // Advanced query methods
        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsFeatured && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 20)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return new List<Product>();

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId &&
                           p.Id != productId &&
                           p.IsActive &&
                           !p.IsDeleted)
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive && !p.IsDeleted && p.AverageRating > 0)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.Reviews.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= threshold && p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }
    }
}