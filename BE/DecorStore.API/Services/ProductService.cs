using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace DecorStore.API.Services
{
    public class ProductService(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper) : IProductService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IImageService _imageService = imageService;
        private readonly IMapper _mapper = mapper;
        private readonly string _folderImageName = "products";

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await _unitOfWork.Products.GetAllAsync(filter);
        }

        public async Task<Product> CreateAsync(CreateProductDTO productDto)
        {
            // Verify category exists
            _ = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            // Map DTO to entity
            var product = _mapper.Map<Product>(productDto);

            // Upload images
            if (productDto.Images != null && productDto.Images.Count > 0)
            {
                var images = new List<Image>();
                foreach (var formFile in productDto.Images)
                {
                    var imagePath = await _imageService.UploadImageAsync(formFile, _folderImageName);
                    images.Add(new Image { FilePath = imagePath, FileName = formFile.FileName });
                }
                product.Images = images;
            }

            await _unitOfWork.Products.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(int id, UpdateProductDTO productDto)
        {
            // Verify category exists
            _ = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            // Get product or throw if not found
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // Map DTO to entity
            _mapper.Map(productDto, product);

            // Handle existing images deletion
            if (productDto.ExistingImages != null && productDto.ExistingImages.Length > 0)
            {
                if (product.Images != null)
                {
                    foreach (var img in product.Images)
                    {
                        if (!productDto.ExistingImages.Contains(img.FilePath))
                        {
                            await _imageService.DeleteImageAsync(img.FilePath);
                        }
                    }
                }
            }

            // Handle new images upload
            if (productDto.NewImages != null && productDto.NewImages.Count > 0)
            {
                var images = new List<Image>();
                foreach (var formFile in productDto.NewImages)
                {
                    var imagePath = await _imageService.UploadImageAsync(formFile, _folderImageName);
                    images.Add(new Image { FilePath = imagePath, FileName = formFile.FileName });
                }
                // Add new images to the product
                if (product.Images == null)
                {
                    product.Images = images;
                }
                else
                {
                    // Add each image individually since ICollection doesn't have AddRange
                    foreach (var image in images)
                    {
                        product.Images.Add(image);
                    }
                }
            }

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var filter = new ProductFilterDTO { PageNumber = 1, PageSize = 100 };
            var products = await _unitOfWork.Products.GetAllAsync(filter);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDTO>(product) : null;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // Delete images from storage
            if (product.Images != null && product.Images.Count > 0)
            {
                foreach (var img in product.Images)
                {
                    await _imageService.DeleteImageAsync(img.FilePath);
                }
            }

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddImageToProductAsync(int productId, IFormFile image)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("Image file cannot be null or empty");
            }

            // Upload the image
            var imagePath = await _imageService.UploadImageAsync(image, _folderImageName);

            // Create new Image entity
            var newImage = new Image
            {
                FileName = image.FileName,
                FilePath = imagePath,
                ProductId = productId,
                AltText = Path.GetFileNameWithoutExtension(image.FileName) // Default alt text
            };

            // Add the image to the product
            product.Images.Add(newImage);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveImageFromProductAsync(int productId, int imageId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                throw new NotFoundException("Image not found");
            }

            // Delete the image file from storage
            await _imageService.DeleteImageAsync(image.FilePath);

            // Remove the image from the product
            product.Images.Remove(image);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products)
        {
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }
    }
}