using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using DecorStore.API.Exceptions;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly string _uploadDirectory;

        public BannerService(IBannerRepository bannerRepository, IHttpContextAccessor httpContextAccessor)
        {
            _bannerRepository = bannerRepository;
            // Đặt thư mục uploads trong wwwroot
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "banners");
            
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<IEnumerable<BannerDTO>> GetAllBannersAsync()
        {
            var banners = await _bannerRepository.GetAllAsync();
            return banners.Select(MapBannerToDto);
        }

        public async Task<IEnumerable<BannerDTO>> GetActiveBannersAsync()
        {
            var banners = await _bannerRepository.GetActiveAsync();
            return banners.Select(MapBannerToDto);
        }

        public async Task<BannerDTO> GetBannerByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return null;
                
            return MapBannerToDto(banner);
        }

        public async Task<Banner> CreateBannerAsync(CreateBannerDTO bannerDto)
        {
            if (bannerDto.ImageFile == null)
                throw new InvalidOperationException("Banner image is required");
                
            var banner = new Banner
            {
                Title = bannerDto.Title,
                Link = bannerDto.Link ?? string.Empty,
                IsActive = bannerDto.IsActive,
                DisplayOrder = bannerDto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };
            
            // Xử lý upload ảnh
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
            string filePath = Path.Combine(_uploadDirectory, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bannerDto.ImageFile.CopyToAsync(stream);
            }
            
            banner.ImageUrl = "/uploads/banners/" + fileName;
            
            return await _bannerRepository.CreateAsync(banner);
        }

        public async Task UpdateBannerAsync(int id, UpdateBannerDTO bannerDto)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new NotFoundException("Banner not found");
                
            banner.Title = bannerDto.Title;
            banner.Link = bannerDto.Link ?? string.Empty;
            banner.IsActive = bannerDto.IsActive;
            banner.DisplayOrder = bannerDto.DisplayOrder;
            
            // Xử lý upload ảnh mới nếu có
            if (bannerDto.ImageFile != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                        banner.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }
                
                // Lưu ảnh mới
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
                string filePath = Path.Combine(_uploadDirectory, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerDto.ImageFile.CopyToAsync(stream);
                }
                
                banner.ImageUrl = "/uploads/banners/" + fileName;
            }
            
            await _bannerRepository.UpdateAsync(banner);
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new NotFoundException("Banner not found");
                
            // Xóa file ảnh nếu có
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                    banner.ImageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            
            await _bannerRepository.DeleteAsync(id);
        }

        private BannerDTO MapBannerToDto(Banner banner)
        {
            if (banner == null)
                return null;
                
            return new BannerDTO
            {
                Id = banner.Id,
                Title = banner.Title,
                ImageUrl = banner.ImageUrl,
                Link = banner.Link,
                IsActive = banner.IsActive,
                DisplayOrder = banner.DisplayOrder,
                CreatedAt = banner.CreatedAt
            };
        }
    }
} 