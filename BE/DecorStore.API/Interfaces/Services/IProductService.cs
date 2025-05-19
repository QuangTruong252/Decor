using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter);
        Task<Product> CreateAsync(CreateProductDTO productDto);
        Task UpdateAsync(int id, UpdateProductDTO productDto);
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> AddImageToProductAsync(int productId, IFormFile image);
        Task<bool> RemoveImageFromProductAsync(int productId, int imageId);
        IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products);
    }
}