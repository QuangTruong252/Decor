using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using AutoMapper;

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
                var images = new string[productDto.Images.Count];
                for (int i = 0; i < productDto.Images.Count; i++)
                {
                    var image = await _imageService.UploadImageAsync(productDto.Images[i], _folderImageName);
                    images[i] = image;
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

            // Handle image update if new image is provided
            if (productDto.Images != null && productDto.Images.Count > 0)
            {
                var images = new string[productDto.Images.Count];
                for (int i = 0; i < productDto.Images.Count; i++)
                {
                    var image = await _imageService.UpdateImageAsync(product.Images[i], productDto.Images[i], _folderImageName);
                    images[i] = image;
                }
                product.Images = images;
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
            if (product.Images != null && product.Images.Length > 0)
            {
                foreach (var image in product.Images)
                {
                    await _imageService.DeleteImageAsync(image);
                }
            }

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}