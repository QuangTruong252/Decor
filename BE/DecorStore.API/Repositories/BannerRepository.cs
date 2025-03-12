using DecorStore.API.Data;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class BannerRepository : IBannerRepository
    {
        private readonly ApplicationDbContext _context;
        
        public BannerRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Banner>> GetAllAsync()
        {
            return await _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Banner>> GetActiveAsync()
        {
            return await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }
        
        public async Task<Banner> GetByIdAsync(int id)
        {
            return await _context.Banners.FindAsync(id);
        }
        
        public async Task<Banner> CreateAsync(Banner banner)
        {
            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();
            return banner;
        }
        
        public async Task UpdateAsync(Banner banner)
        {
            _context.Banners.Update(banner);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                banner.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
} 