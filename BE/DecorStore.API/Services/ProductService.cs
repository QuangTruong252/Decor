using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Repositories;

namespace DecorStore.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => MapProductToDto(p));
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapProductToDto(product) : null;
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Category = productDto.Category,
                ImageUrl = productDto.ImageUrl
            };

            var createdProduct = await _productRepository.CreateAsync(product);
            return MapProductToDto(createdProduct);
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO productDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            
            if (existingProduct == null)
                return null;

            existingProduct.Name = productDto.Name;
            existingProduct.Price = productDto.Price;
            existingProduct.Category = productDto.Category;
            existingProduct.ImageUrl = productDto.ImageUrl;

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            return MapProductToDto(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(string category)
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            return products.Select(p => MapProductToDto(p));
        }

        // Helper method to map Product entity to ProductDTO
        private ProductDTO MapProductToDto(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category,
                ImageUrl = product.ImageUrl
            };
        }
    }
} 