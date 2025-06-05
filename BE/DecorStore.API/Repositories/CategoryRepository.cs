using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Models;
using DecorStore.API.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Category>> GetPagedAsync(CategoryFilterDTO filter)
        {
            var query = GetFilteredCategories(filter);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Category>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync()
        {
            return await _context.Categories
                .Include(c => c.Subcategories)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => c.ParentId == null)
                .ToListAsync();
        }

        public async Task<Category> GetByIdWithChildrenAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Subcategories)
                    .ThenInclude(sc => sc.CategoryImages)
                    .ThenInclude(ci => ci.Image)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Subcategories)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Categories.AnyAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug, int excludeCategoryId)
        {
            return await _context.Categories.AnyAsync(c => c.Slug == slug && !c.IsDeleted && c.Id != excludeCategoryId);
        }

        public async Task<int> GetTotalCountAsync(CategoryFilterDTO filter)
        {
            return await GetFilteredCategories(filter).CountAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    CategoryImages = c.CategoryImages,
                    Products = c.Products.Where(p => !p.IsDeleted).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => c.ParentId == parentId)
                .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Category>> GetPopularCategoriesAsync(int count = 10)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .OrderByDescending(c => c.Products.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllForExcelExportAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .ToListAsync();
        }

        private IQueryable<Category> GetFilteredCategories(CategoryFilterDTO filter)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm)
                );
            }

            if (filter.ParentId.HasValue)
            {
                query = query.Where(c => c.ParentId == filter.ParentId);
            }

            if (filter.IsRootCategory.HasValue)
            {
                query = filter.IsRootCategory.Value
                    ? query.Where(c => c.ParentId == null)
                    : query.Where(c => c.ParentId != null);
            }

            if (filter.IncludeSubcategories)
            {
                query = query.Include(c => c.Subcategories);
            }

            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filter.CreatedAfter);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filter.CreatedBefore);
            }

            if (!filter.IncludeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name_asc" => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                "date_asc" => query.OrderBy(c => c.CreatedAt),
                "date_desc" => query.OrderByDescending(c => c.CreatedAt),
                _ => query.OrderBy(c => c.Name)
            };

            return query;
        }
    }
}
