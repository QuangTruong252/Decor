using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

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
            var query = GetFilteredProducts(filter);
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await GetFilteredProducts(filter).ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<int> GetTotalCountAsync(ProductFilterDTO filter)
        {
            return await GetFilteredProducts(filter).CountAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Products.AnyAsync(p => p.Slug == slug && !p.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug, int excludeProductId)
        {
            return await _context.Products.AnyAsync(p => p.Slug == slug && !p.IsDeleted && p.Id != excludeProductId);
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            return await _context.Products.AnyAsync(p => p.SKU == sku && !p.IsDeleted);
        }

        public async Task<bool> SkuExistsAsync(string sku, int excludeProductId)
        {
            return await _context.Products.AnyAsync(p => p.SKU == sku && !p.IsDeleted && p.Id != excludeProductId);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                _context.Entry(product).State = EntityState.Modified;
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
            foreach (var product in products)
            {
                product.IsDeleted = true;
            }
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.IsFeatured)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 20)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.CategoryId == categoryId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return new List<Product>();

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.StockQuantity <= threshold)
                .ToListAsync();
        }

        private IQueryable<Product> GetFilteredProducts(ProductFilterDTO filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm)
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
                query = query.Where(p => p.SKU.Contains(filter.SKU));

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
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return query;
        }
    }
}
