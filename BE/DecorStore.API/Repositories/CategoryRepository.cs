using DecorStore.API.Data;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Subcategories)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> GetByIdWithChildrenAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Categories.AnyAsync(c => c.Slug == slug);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsDeleted = true;
            }
        }

        public async Task<PagedResult<Category>> GetPagedAsync(CategoryFilterDTO filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var query = BuildCategoryQuery(filter);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var items = await query
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Category>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<int> GetTotalCountAsync(CategoryFilterDTO filter)
        {
            var query = BuildCategoryQuery(filter);
            return await query.CountAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<IEnumerable<Category>> GetPopularCategoriesAsync(int count = 10)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Products.Count(p => !p.IsDeleted))
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllForExcelExportAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Subcategories)
                .Include(c => c.Products.Where(p => !p.IsDeleted))
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        private IQueryable<Category> BuildCategoryQuery(CategoryFilterDTO filter)
        {
            var query = _context.Categories.AsQueryable();

            // Apply base filters
            if (!filter.IncludeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm));
            }

            // Apply parent filter
            if (filter.ParentId.HasValue)
            {
                query = query.Where(c => c.ParentId == filter.ParentId.Value);
            }

            // Apply root category filter
            if (filter.IsRootCategory.HasValue)
            {
                if (filter.IsRootCategory.Value)
                {
                    query = query.Where(c => c.ParentId == null);
                }
                else
                {
                    query = query.Where(c => c.ParentId != null);
                }
            }

            // Apply date range filters
            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filter.CreatedAfter.Value);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filter.CreatedBefore.Value);
            }

            // Include related data based on filter options
            if (filter.IncludeSubcategories)
            {
                query = query.Include(c => c.Subcategories);
            }

            if (filter.IncludeProductCount)
            {
                query = query.Include(c => c.Products);
            }

            return query;
        }

        private IQueryable<Category> ApplySorting(IQueryable<Category> query, PaginationParameters filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
            {
                return query.OrderBy(c => c.Name);
            }

            return filter.SortBy.ToLower() switch
            {
                "name" => filter.IsDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "createdat" => filter.IsDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                "productcount" => filter.IsDescending ?
                    query.OrderByDescending(c => c.Products.Count(p => !p.IsDeleted)) :
                    query.OrderBy(c => c.Products.Count(p => !p.IsDeleted)),
                _ => query.OrderBy(c => c.Name)
            };
        }
    }
}