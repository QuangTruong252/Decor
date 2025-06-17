using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Interfaces;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Controllers.Base;
using AutoMapper;

namespace DecorStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : BaseController
    {
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public ImageController(IImageService imageService, IMapper mapper, ILogger<ImageController> logger) 
            : base(logger)
        {
            _imageService = imageService;
            _mapper = mapper;
        }

        /// <summary>
        /// Upload multiple images and return their IDs
        /// </summary>
        /// <param name="uploadDto">Images to upload</param>
        /// <returns>List of uploaded image information</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImages([FromForm] ImageUploadDTO uploadDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            if (uploadDto.Files == null || !uploadDto.Files.Any())
            {
                return BadRequest("No files provided");
            }

            // Get or create images and return their IDs
            var imageIdsResult = await _imageService.GetOrCreateImagesAsync(uploadDto.Files, uploadDto.folderName ?? "images");
            if (imageIdsResult.IsFailure)
            {
                return BadRequest(imageIdsResult.Error);
            }
            
            // Get the full image details
            var imagesResult = await _imageService.GetImagesByIdsAsync(imageIdsResult.Data!);
            if (imagesResult.IsFailure)
            {
                return BadRequest(imagesResult.Error);
            }
            
            // Map to response DTOs
            var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(imagesResult.Data);

            var response = new ImageUploadResponseDTO
            {
                Images = imageResponseDtos
            };

            return CreatedAtAction(nameof(GetImagesByIds), new { ids = string.Join(",", imageIdsResult.Data!) }, response);
        }

        /// <summary>
        /// Get all images in the system
        /// </summary>
        /// <returns>List of all images</returns>
        [HttpGet("system")]
        public async Task<ActionResult<ImageUploadResponseDTO>> GetSystemImages()
        {
            var allImagesResult = await _imageService.GetAllImagesAsync();
            if (allImagesResult.IsFailure)
            {
                return BadRequest(allImagesResult.Error);
            }

            var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(allImagesResult.Data);
            
            var response = new ImageUploadResponseDTO
            {
                Images = imageResponseDtos
            };

            return Ok(response);
        }

        /// <summary>
        /// Check if an image exists in the system by filename
        /// </summary>
        /// <param name="fileName">The filename to check</param>
        /// <returns>Boolean indicating if image exists</returns>
        [HttpGet("exists/{fileName}")]
        public async Task<ActionResult<bool>> CheckImageExists(string fileName)
        {
            var existsResult = await _imageService.ImageExistsInSystemAsync(fileName);
            if (existsResult.IsFailure)
                return BadRequest(existsResult.Error);
                
            return existsResult.Data;
        }

        /// <summary>
        /// Get images by their IDs
        /// </summary>
        /// <param name="ids">Comma-separated list of image IDs</param>
        /// <returns>List of images</returns>
        [HttpGet("by-ids/{ids}")]
        public async Task<ActionResult<ImageUploadResponseDTO>> GetImagesByIds(string ids)
        {
            // Parse comma-separated IDs
            var imageIds = ids.Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            if (!imageIds.Any())
            {
                return BadRequest("No valid image IDs provided");
            }

            var imagesResult = await _imageService.GetImagesByIdsAsync(imageIds);
            if (imagesResult.IsFailure)
            {
                return BadRequest(imagesResult.Error);
            }

            var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(imagesResult.Data);

            var response = new ImageUploadResponseDTO
            {
                Images = imageResponseDtos
            };

            return Ok(response);
        }

        [HttpGet("get-by-filepaths")]
        public async Task<ActionResult<ImageUploadResponseDTO>> GetImagesByFilePaths([FromQuery] List<string> filePaths)
        {
            var imagesResult = await _imageService.GetImagesByFilePathsAsync(filePaths);
            if (imagesResult.IsFailure)
            {
                return BadRequest(imagesResult.Error);
            }

            var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(imagesResult.Data);

            var response = new ImageUploadResponseDTO
            {
                Images = imageResponseDtos
            };

            return Ok(response);
        }
    }
}
