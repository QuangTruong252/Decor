using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IBannerRepository
    {
        Task<IEnumerable<Banner>> GetAllAsync();
        Task<IEnumerable<Banner>> GetActiveAsync();
        Task<Banner> GetByIdAsync(int id);
        Task<Banner> CreateAsync(Banner banner);
        Task UpdateAsync(Banner banner);
        Task DeleteAsync(int id);
    }
}
