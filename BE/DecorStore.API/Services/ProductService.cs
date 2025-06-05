using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
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

        public async Task<PagedResult<ProductDTO>> GetPagedProductsAsync(ProductFilterDTO filter)
        {
            var pagedProducts = await _unitOfWork.Products.GetPagedAsync(filter);
            var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(pagedProducts.Items);

            return new PagedResult<ProductDTO>(productDtos, pagedProducts.Pagination.TotalCount,
                pagedProducts.Pagination.CurrentPage, pagedProducts.Pagination.PageSize);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(ProductFilterDTO filter)
        {
            return await _unitOfWork.Products.GetAllAsync(filter);
        }

        public async Task<Product> CreateAsync(CreateProductDTO productDto)
        {
            // Verify category exists
            _ = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            // Validate unique constraints
            if (await _unitOfWork.Products.SkuExistsAsync(productDto.SKU))
                throw new ArgumentException("SKU already exists");

            if (await _unitOfWork.Products.SlugExistsAsync(productDto.Slug))
                throw new ArgumentException("Slug already exists");

            // Map DTO to entity
            var product = _mapper.Map<Product>(productDto);

            // Create the product first
            await _unitOfWork.Products.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Handle image assignment using ImageIds via many-to-many relationship
            if (productDto.ImageIds != null && productDto.ImageIds.Count > 0)
            {
                // Verify that all image IDs exist
                var images = await _imageService.GetImagesByIdsAsync(productDto.ImageIds);
                
                if (images.Count() != productDto.ImageIds.Count)
                {
                    var foundIds = images.Select(img => img.Id).ToList();
                    var missingIds = productDto.ImageIds.Except(foundIds).ToList();
                    throw new NotFoundException($"The following image IDs were not found: {string.Join(", ", missingIds)}");
                }

                // Associate images with the product using junction table
                foreach (var imageId in productDto.ImageIds)
                {
                    await _unitOfWork.Images.AddProductImageAsync(imageId, product.Id);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            
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

            // Validate unique constraints if fields are being changed
            if (product.SKU != productDto.SKU && await _unitOfWork.Products.SkuExistsAsync(productDto.SKU, id))
                throw new ArgumentException("SKU already exists");

            if (product.Slug != productDto.Slug && await _unitOfWork.Products.SlugExistsAsync(productDto.Slug, id))
                throw new ArgumentException("Slug already exists");

            // Map basic product properties (excluding images)
            _mapper.Map(productDto, product);

            // Handle image associations: Remove old associations and create new ones
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // Step 1: Remove all existing image associations for this product
                    var existingProductImages = await _unitOfWork.Images.GetProductImagesByProductIdAsync(id);
                    foreach (var productImage in existingProductImages)
                    {
                        _unitOfWork.Images.RemoveProductImage(productImage.ImageId, productImage.ProductId);
                    }

                    // Step 2: Associate new images if provided
                    if (productDto.ImageIds != null && productDto.ImageIds.Count > 0)
                    {
                        // Verify that all image IDs exist
                        var newImages = await _imageService.GetImagesByIdsAsync(productDto.ImageIds);
                        
                        if (newImages.Count() != productDto.ImageIds.Count)
                        {
                            var foundIds = newImages.Select(img => img.Id).ToList();
                            var missingIds = productDto.ImageIds.Except(foundIds).ToList();
                            throw new NotFoundException($"The following image IDs were not found: {string.Join(", ", missingIds)}");
                        }
                        
                        // Associate new images with this product using junction table
                        foreach (var imageId in productDto.ImageIds)
                        {
                            await _unitOfWork.Images.AddProductImageAsync(imageId, product.Id);
                        }
                    }

                    // Step 3: Update product and save changes
                    await _unitOfWork.Products.UpdateAsync(product);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    
                    return Task.CompletedTask; // Return type for ExecuteWithExecutionStrategyAsync
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });
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

        public async Task<bool> BulkDeleteProductsAsync(BulkDeleteDTO bulkDeleteDto)
        {
            if (bulkDeleteDto.Ids == null || !bulkDeleteDto.Ids.Any())
                throw new ArgumentException("No product IDs provided for deletion");

            // Use execution strategy to handle retries with transactions
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Get all products to delete
                    var productsToDelete = new List<Product>();
                    foreach (var id in bulkDeleteDto.Ids)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(id);
                        if (product != null)
                        {
                            productsToDelete.Add(product);
                        }
                    }

                    // Delete images from storage
                    foreach (var product in productsToDelete)
                    {
                        if (product.Images != null && product.Images.Count > 0)
                        {
                            foreach (var img in product.Images)
                            {
                                await _imageService.DeleteImageAsync(img.FilePath);
                            }
                        }
                    }

                    // Mark products as deleted in the database
                    await _unitOfWork.Products.BulkDeleteAsync(bulkDeleteDto.Ids);
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();
                    return true;
                }
                catch (Exception)
                {
                    // Rollback transaction on error
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            });

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
                AltText = Path.GetFileNameWithoutExtension(image.FileName) // Default alt text
            };

            // Add the image to database first
            await _unitOfWork.Images.AddAsync(newImage);
            await _unitOfWork.SaveChangesAsync();

            // Associate the image with the product using junction table
            await _unitOfWork.Images.AddProductImageAsync(newImage.Id, productId);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> RemoveImageFromProductAsync(int productId, int imageId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            // Check if the image is associated with this product
            var productImages = await _unitOfWork.Images.GetProductImagesByProductIdAsync(productId);
            var productImage = productImages.FirstOrDefault(pi => pi.ImageId == imageId);
            
            if (productImage == null)
            {
                throw new NotFoundException("Image not associated with this product");
            }

            // Get the image to delete the file
            var image = await _unitOfWork.Images.GetByIdAsync(imageId);
            if (image != null)
            {
                // Delete the image file from storage
                await _imageService.DeleteImageAsync(image.FilePath);
            }

            // Remove the association between product and image
            _unitOfWork.Images.RemoveProductImage(imageId, productId);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        public IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products)
        {
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        // Advanced query methods
        public async Task<IEnumerable<ProductDTO>> GetFeaturedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int categoryId, int count = 20)
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId, count);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            var products = await _unitOfWork.Products.GetRelatedProductsAsync(productId, count);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetTopRatedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetTopRatedProductsAsync(count);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync(int threshold = 10)
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }
    }
}
