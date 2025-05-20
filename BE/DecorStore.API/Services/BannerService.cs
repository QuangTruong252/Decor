using System;
using System.Collections.Generic;
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
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly string _folderName = "banners";

        public BannerService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
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

            try
            {
                // Upload image using ImageService
                string imagePath = await _imageService.UploadImageAsync(bannerDto.ImageFile, _folderName);

                // Set the image URL in the banner entity
                banner.ImageUrl = imagePath;

                // Save banner to database
                await _unitOfWork.Banners.CreateAsync(banner);
                await _unitOfWork.SaveChangesAsync();
                return banner;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create banner: {ex.Message}", ex);
            }
        }

        public async Task UpdateBannerAsync(int id, UpdateBannerDTO bannerDto)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null)
                throw new NotFoundException("Banner not found");

            // Map DTO to entity
            _mapper.Map(bannerDto, banner);

            try
            {
                // Update image if a new one is provided
                if (bannerDto.ImageFile != null)
                {
                    // Use ImageService to update the image
                    string newImagePath = await _imageService.UpdateImageAsync(banner.ImageUrl, bannerDto.ImageFile, _folderName);

                    // Update the image URL in the banner entity
                    banner.ImageUrl = newImagePath;
                }

                // Save changes to database
                await _unitOfWork.Banners.UpdateAsync(banner);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update banner: {ex.Message}", ex);
            }
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _unitOfWork.Banners.GetByIdAsync(id);
            if (banner == null)
                throw new NotFoundException("Banner not found");

            try
            {
                // Delete the image using ImageService
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(banner.ImageUrl);
                }

                // Delete the banner from database
                await _unitOfWork.Banners.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete banner: {ex.Message}", ex);
            }
        }
    }
}