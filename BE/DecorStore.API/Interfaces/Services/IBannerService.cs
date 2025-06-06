using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface IBannerService
    {
        Task<Result<IEnumerable<BannerDTO>>> GetAllBannersAsync();
        Task<Result<IEnumerable<BannerDTO>>> GetActiveBannersAsync();
        Task<Result<BannerDTO>> GetBannerByIdAsync(int id);
        Task<Result<BannerDTO>> CreateBannerAsync(CreateBannerDTO bannerDto);
        Task<Result<BannerDTO>> UpdateBannerAsync(int id, UpdateBannerDTO bannerDto);
        Task<Result> DeleteBannerAsync(int id);
    }
}
