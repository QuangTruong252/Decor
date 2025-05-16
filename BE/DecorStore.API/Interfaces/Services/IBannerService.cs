using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerDTO>> GetAllBannersAsync();
        Task<IEnumerable<BannerDTO>> GetActiveBannersAsync();
        Task<BannerDTO?> GetBannerByIdAsync(int id);
        Task<Banner> CreateBannerAsync(CreateBannerDTO bannerDto);
        Task UpdateBannerAsync(int id, UpdateBannerDTO bannerDto);
        Task DeleteBannerAsync(int id);
    }
}