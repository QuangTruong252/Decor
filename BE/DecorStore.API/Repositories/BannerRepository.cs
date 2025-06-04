using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
            return await _context.Banners.ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetActiveAsync()
        {
            return await _context.Banners
                .Where(b => b.IsActive && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<Banner> GetByIdAsync(int id)
        {
            return await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Banner> CreateAsync(Banner banner)
        {
            await _context.Banners.AddAsync(banner);
            return banner;
        }

        public async Task UpdateAsync(Banner banner)
        {
            _context.Entry(banner).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var banner = await GetByIdAsync(id);
            if (banner != null)
            {
                banner.IsDeleted = true;
                await UpdateAsync(banner);
            }
        }
    }
}
