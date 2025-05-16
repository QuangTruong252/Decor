using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using Microsoft.AspNetCore.Http;
using DecorStore.API.Interfaces;
using AutoMapper;

namespace DecorStore.API.Services
{
    public class BannerService : IBannerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _uploadDirectory;
        private readonly IMapper _mapper;

        public BannerService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "banners");

            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<IEnumerable<BannerDTO>> GetAllBannersAsync()
        {
            var banners = await _unitOfWork.Banners.GetAllAsync();
            return _mapper.Map<IEnumerable<BannerDTO>>(banners);
        }

        public async Task<IEnumerable<BannerDTO>> GetActiveBannersAsync()
        {
            var banners = await _unitOfWork.Banners.GetActiveAsync();
            return _mapper.Map<IEnumerable<BannerDTO>>(banners);
        }

        public async Task<BannerDTO?> GetBannerByIdAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null)
                return null;

            return _mapper.Map<BannerDTO>(banner);
        }

        public async Task<Banner> CreateBannerAsync(CreateBannerDTO bannerDto)
        {
            if (bannerDto.ImageFile == null)
                throw new InvalidOperationException("Banner image is required");

            // Map DTO to entity
            var banner = _mapper.Map<Banner>(bannerDto);

            // Xử lý upload ảnh
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerDto.ImageFile.FileName);
            string filePath = Path.Combine(_uploadDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bannerDto.ImageFile.CopyToAsync(stream);
            }

            banner.ImageUrl = "/uploads/banners/" + fileName;

            await _unitOfWork.Banners.CreateAsync(banner);
            await _unitOfWork.SaveChangesAsync();
            return banner;
        }

        public async Task UpdateBannerAsync(int id, UpdateBannerDTO bannerDto)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null)
                throw new NotFoundException("Banner not found");

            // Map DTO to entity
            _mapper.Map(bannerDto, banner);

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

            await _unitOfWork.Banners.UpdateAsync(banner);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
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

            await _unitOfWork.Banners.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}