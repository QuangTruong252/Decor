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
{    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly ICacheService _cacheService;
        
        public CategoryRepository(ApplicationDbContext context, ICacheService cacheService) : base(context)
        {
            _cacheService = cacheService;
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
        }        public async Task<IEnumerable<Category>> GetRootCategoriesWithChildrenAsync()
        {
            const string cacheKey = "categories:root-with-children";
            
            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _context.Categories
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(c => c.Subcategories.Where(sub => !sub.IsDeleted))
                    .Include(c => c.CategoryImages)
                    .ThenInclude(ci => ci.Image)
                    .Where(c => c.ParentId == null && !c.IsDeleted)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();
            }, TimeSpan.FromMinutes(60), CacheItemPriority.High, "categories");
        }

        public async Task<Category?> GetByIdWithChildrenAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.ParentCategory)
                .Include(c => c.Subcategories.Where(sub => !sub.IsDeleted))
                    .ThenInclude(sc => sc.CategoryImages)
                    .ThenInclude(ci => ci.Image)
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.ParentCategory)
                .Include(c => c.Subcategories.Where(sub => !sub.IsDeleted))
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<bool> SlugExistsAsync(string slug, int excludeCategoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Slug == slug && !c.IsDeleted && c.Id != excludeCategoryId);
        }

        public async Task<int> GetTotalCountAsync(CategoryFilterDTO filter)
        {
            return await GetFilteredCategories(filter).CountAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Products.Where(p => !p.IsDeleted && p.IsActive))
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubcategoriesAsync(int parentId)
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => c.ParentId == parentId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive);
        }

        public async Task<IEnumerable<Category>> GetPopularCategoriesAsync(int count = 10)
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Products.Where(p => !p.IsDeleted && p.IsActive))
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Products.Count(p => !p.IsDeleted && p.IsActive))
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllForExcelExportAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.ParentCategory)
                .Include(c => c.Products.Where(p => !p.IsDeleted))
                .Include(c => c.CategoryImages)
                .ThenInclude(ci => ci.Image)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        private IQueryable<Category> GetFilteredCategories(CategoryFilterDTO filter)
        {
            var query = _context.Categories
                .AsNoTracking()
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

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Name == name && !c.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeCategoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Name == name && c.Id != excludeCategoryId && !c.IsDeleted);
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<bool> ExistsBySlugAsync(string slug, int excludeCategoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Slug == slug && c.Id != excludeCategoryId && !c.IsDeleted);
        }

        public async Task<List<int>> GetDescendantIdsAsync(int categoryId)
        {
            var descendantIds = new List<int>();
            await GetDescendantIdsRecursive(categoryId, descendantIds);
            return descendantIds;
        }

        private async Task GetDescendantIdsRecursive(int categoryId, List<int> descendantIds)
        {
            var childrenIds = await _context.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == categoryId && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var childId in childrenIds)
            {
                descendantIds.Add(childId);
                await GetDescendantIdsRecursive(childId, descendantIds);
            }
        }
    }
}
