using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;
using System;
using DecorStore.API.Exceptions;

namespace DecorStore.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await _productRepository.GetAllAsync(filter);
        }

        public async Task<Product> CreateAsync(CreateProductDTO productDto)
        {
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
                IsActive = productDto.IsActive
            };

            return await _productRepository.CreateAsync(product);
        }

        public async Task UpdateAsync(int id, UpdateProductDTO productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found");

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
                Images = product.Images?.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl,
                    IsDefault = i.IsDefault
                }).ToList() ?? new List<ProductImageDTO>()
            };
        }
    }
} 