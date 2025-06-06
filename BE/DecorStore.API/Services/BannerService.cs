using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using Microsoft.AspNetCore.Http;
using DecorStore.API.Interfaces;
using DecorStore.API.Common;
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

        public async Task<Result<IEnumerable<BannerDTO>>> GetAllBannersAsync()
        {
            try
            {
                var banners = await _unitOfWork.Banners.GetAllAsync();
                var bannerDtos = _mapper.Map<IEnumerable<BannerDTO>>(banners);
                return Result<IEnumerable<BannerDTO>>.Success(bannerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BannerDTO>>.Failure($"Failed to retrieve banners: {ex.Message}", "RETRIEVAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<BannerDTO>>> GetActiveBannersAsync()
        {
            try
            {
                var banners = await _unitOfWork.Banners.GetActiveAsync();
                var bannerDtos = _mapper.Map<IEnumerable<BannerDTO>>(banners);
                return Result<IEnumerable<BannerDTO>>.Success(bannerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BannerDTO>>.Failure($"Failed to retrieve active banners: {ex.Message}", "RETRIEVAL_ERROR");
            }
        }

        public async Task<Result<BannerDTO>> GetBannerByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Result<BannerDTO>.Failure("Invalid banner ID", "INVALID_INPUT");
            }

            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null)
                {
                    return Result<BannerDTO>.NotFound("Banner");
                }

                var bannerDto = _mapper.Map<BannerDTO>(banner);
                return Result<BannerDTO>.Success(bannerDto);
            }
            catch (Exception ex)
            {
                return Result<BannerDTO>.Failure($"Failed to retrieve banner: {ex.Message}", "RETRIEVAL_ERROR");
            }
        }

        public async Task<Result<BannerDTO>> CreateBannerAsync(CreateBannerDTO bannerDto)
        {
            // Input validation
            if (bannerDto == null)
            {
                return Result<BannerDTO>.Failure("Banner data is required", "INVALID_INPUT");
            }

            if (bannerDto.ImageFile == null)
            {
                return Result<BannerDTO>.Failure("Banner image is required", "INVALID_INPUT");
            }

            if (string.IsNullOrWhiteSpace(bannerDto.Title))
            {
                return Result<BannerDTO>.Failure("Banner title is required", "INVALID_INPUT");
            }

            // Business rule validation
            if (bannerDto.StartDate.HasValue && bannerDto.EndDate.HasValue && bannerDto.StartDate > bannerDto.EndDate)
            {
                return Result<BannerDTO>.Failure("Start date cannot be after end date", "INVALID_DATE_RANGE");
            }

            if (!string.IsNullOrEmpty(bannerDto.LinkUrl) && !Uri.IsWellFormedUriString(bannerDto.LinkUrl, UriKind.RelativeOrAbsolute))
            {
                return Result<BannerDTO>.Failure("Invalid URL format", "INVALID_URL");
            }

            try
            {
                // Map DTO to entity
                var banner = _mapper.Map<Banner>(bannerDto);

                // Upload image using ImageService
                var uploadResult = await _imageService.UploadImageAsync(bannerDto.ImageFile, _folderName);
                if (uploadResult.IsFailure)
                {
                    return Result<BannerDTO>.Failure($"Failed to upload image: {uploadResult.Error}", "IMAGE_UPLOAD_ERROR");
                }

                // Set the image URL in the banner entity
                banner.ImageUrl = uploadResult.Data;

                // Save banner to database
                await _unitOfWork.Banners.CreateAsync(banner);
                await _unitOfWork.SaveChangesAsync();

                var createdBannerDto = _mapper.Map<BannerDTO>(banner);
                return Result<BannerDTO>.Success(createdBannerDto);
            }
            catch (Exception ex)
            {
                return Result<BannerDTO>.Failure($"Failed to create banner: {ex.Message}", "CREATION_ERROR");
            }
        }

        public async Task<Result<BannerDTO>> UpdateBannerAsync(int id, UpdateBannerDTO bannerDto)
        {
            // Input validation
            if (id <= 0)
            {
                return Result<BannerDTO>.Failure("Invalid banner ID", "INVALID_INPUT");
            }

            if (bannerDto == null)
            {
                return Result<BannerDTO>.Failure("Banner data is required", "INVALID_INPUT");
            }

            if (string.IsNullOrWhiteSpace(bannerDto.Title))
            {
                return Result<BannerDTO>.Failure("Banner title is required", "INVALID_INPUT");
            }

            // Business rule validation
            if (bannerDto.StartDate.HasValue && bannerDto.EndDate.HasValue && bannerDto.StartDate > bannerDto.EndDate)
            {
                return Result<BannerDTO>.Failure("Start date cannot be after end date", "INVALID_DATE_RANGE");
            }

            if (!string.IsNullOrEmpty(bannerDto.LinkUrl) && !Uri.IsWellFormedUriString(bannerDto.LinkUrl, UriKind.RelativeOrAbsolute))
            {
                return Result<BannerDTO>.Failure("Invalid URL format", "INVALID_URL");
            }

            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null)
                {
                    return Result<BannerDTO>.NotFound("Banner");
                }

                // Map DTO to entity
                _mapper.Map(bannerDto, banner);

                // Update image if a new one is provided
                if (bannerDto.ImageFile != null)
                {
                    var updateResult = await _imageService.UpdateImageAsync(banner.ImageUrl, bannerDto.ImageFile, _folderName);
                    if (updateResult.IsFailure)
                    {
                        return Result<BannerDTO>.Failure($"Failed to update image: {updateResult.Error}", "IMAGE_UPDATE_ERROR");
                    }

                    // Update the image URL in the banner entity
                    banner.ImageUrl = updateResult.Data;
                }

                // Save changes to database
                await _unitOfWork.Banners.UpdateAsync(banner);
                await _unitOfWork.SaveChangesAsync();

                var updatedBannerDto = _mapper.Map<BannerDTO>(banner);
                return Result<BannerDTO>.Success(updatedBannerDto);
            }
            catch (Exception ex)
            {
                return Result<BannerDTO>.Failure($"Failed to update banner: {ex.Message}", "UPDATE_ERROR");
            }
        }

        public async Task<Result> DeleteBannerAsync(int id)
        {
            // Input validation
            if (id <= 0)
            {
                return Result.Failure("Invalid banner ID", "INVALID_INPUT");
            }

            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null)
                {
                    return Result.NotFound("Banner");
                }

                // Delete the image using ImageService
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                {
                    var deleteImageResult = await _imageService.DeleteImageAsync(banner.ImageUrl);
                    if (deleteImageResult.IsFailure)
                    {
                        return Result.Failure($"Failed to delete image: {deleteImageResult.Error}", "IMAGE_DELETE_ERROR");
                    }
                }

                // Delete the banner from database
                await _unitOfWork.Banners.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to delete banner: {ex.Message}", "DELETE_ERROR");
            }
        }
    }
}
