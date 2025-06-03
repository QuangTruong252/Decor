using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Interfaces;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using AutoMapper;

namespace DecorStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public ImageController(IImageService imageService, IMapper mapper)
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
        public async Task<ActionResult<ImageUploadResponseDTO>> UploadImages([FromForm] ImageUploadDTO uploadDto)
        {
            try
            {
                if (uploadDto.Files == null || !uploadDto.Files.Any())
                {
                    return BadRequest("No files provided");
                }

                // Validate file count (max 10 files at once)
                if (uploadDto.Files.Count > 10)
                {
                    return BadRequest("Maximum 10 files can be uploaded at once");
                }

                // Get or create images and return their IDs
                var imageIds = await _imageService.GetOrCreateImagesAsync(uploadDto.Files, uploadDto.folderName);
                
                // Get the full image details
                var images = await _imageService.GetImagesByIdsAsync(imageIds);
                
                // Map to response DTOs
                var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(images);

                var response = new ImageUploadResponseDTO
                {
                    Images = imageResponseDtos
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while uploading images: {ex.Message}");
            }
        }        /// <summary>
        /// Get all images in the system
        /// </summary>
        /// <returns>List of all images</returns>
        [HttpGet("system")]
        public async Task<ActionResult<ImageUploadResponseDTO>> GetSystemImages()
        {
            try
        {
                var allImages = await _imageService.GetAllImagesAsync();
                var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(allImages);
                
                var response = new ImageUploadResponseDTO
                {
                    Images = imageResponseDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving images: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if an image exists in the system by filename
        /// </summary>
        /// <param name="fileName">The filename to check</param>
        /// <returns>Boolean indicating if image exists</returns>
        [HttpGet("exists/{fileName}")]
        public async Task<ActionResult<bool>> CheckImageExists(string fileName)
        {
            try
            {
                var exists = await _imageService.ImageExistsInSystemAsync(fileName);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while checking image existence: {ex.Message}");
            }
        }

        /// <summary>
        /// Get images by their IDs
        /// </summary>
        /// <param name="ids">Comma-separated list of image IDs</param>
        /// <returns>List of images</returns>
        [HttpGet("{ids}")]
        public async Task<ActionResult<ImageUploadResponseDTO>> GetImagesByIds(string ids)
        {
            try
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

                var images = await _imageService.GetImagesByIdsAsync(imageIds);
                var imageResponseDtos = _mapper.Map<List<ImageResponseDTO>>(images);

                var response = new ImageUploadResponseDTO
                {
                    Images = imageResponseDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving images: {ex.Message}");
            }
        }
    }
}
