using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using System;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using Azure.Core;

namespace DecorStore.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly string _folderImageName = "products";
        public ProductService(IProductRepository productRepository, IImageService imageService, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await _productRepository.GetAllAsync(filter);
        }

        public async Task<Product> CreateAsync(CreateProductDTO productDto)
        {
            var category  = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (category == null)
                throw new NotFoundException("Category not found");
            // upload images
            string[] images = null!;
            if (productDto.Images != null && productDto.Images.Count > 0)
            {
                images = new string[productDto.Images.Count];
                for (int i = 0; i < productDto.Images.Count; i++)
                {
                    var image = await _imageService.UploadImageAsync(productDto.Images[i], _folderImageName);
                    images[i] = image;
                }
            }
            var product = new Product
            {
                Name = productDto.Name,
                Slug = productDto.Slug,
                Description = productDto.Description ?? string.Empty,
                Price = productDto.Price,
                OriginalPrice = productDto.OriginalPrice,
                StockQuantity = productDto.StockQuantity,
                SKU = productDto.SKU,
                CategoryId = productDto.CategoryId,
                IsFeatured = productDto.IsFeatured,
                IsActive = productDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Images = images
            };

            return await _productRepository.CreateAsync(product);
        }

        public async Task UpdateAsync(int id, UpdateProductDTO productDto)
        {
            var category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (category == null)
                throw new NotFoundException("Category not found");
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");
            // Handle image update if new image is provided
            string[] images = null!;
            if (productDto.Images != null && productDto.Images.Count > 0)
            {
                images = new string[productDto.Images.Count];
                for (int i = 0; i < productDto.Images.Count; i++)
                {
                    var image = await _imageService.UpdateImageAsync(product.Images[i], productDto.Images[i], _folderImageName);
                    images[i] = image;
                }
            }

            product.Name = productDto.Name;
            product.Slug = productDto.Slug;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.OriginalPrice = productDto.OriginalPrice;
            product.StockQuantity = productDto.StockQuantity;
            product.SKU = productDto.SKU;
            product.CategoryId = productDto.CategoryId;
            product.IsFeatured = productDto.IsFeatured;
            product.IsActive = productDto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            product.Images = images ?? product.Images;

            await _productRepository.UpdateAsync(product);
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var filter = new ProductFilterDTO { PageNumber = 1, PageSize = 100 };
            var products = await _productRepository.GetAllAsync(filter);
            return products.Select(p => MapProductToDto(p));
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapProductToDto(product) : null;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");
            // Delete images from storage
            if (product.Images != null && product.Images.Length > 0)
            {
                foreach (var image in product.Images)
                {
                    await _imageService.DeleteImageAsync(image);
                }
            }
            await _productRepository.DeleteAsync(id);
            return true;
        }

        // Helper method to map Product entity to ProductDTO
        private ProductDTO MapProductToDto(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                StockQuantity = product.StockQuantity,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                AverageRating = product.AverageRating,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = product.Images ?? Array.Empty<string>()
            };
        }
    }
} 