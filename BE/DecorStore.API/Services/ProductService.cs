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
        }        public async Task<Product> CreateAsync(CreateProductDTO productDto)
        {
            // Verify category exists
            _ = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            // Map DTO to entity
            var product = _mapper.Map<Product>(productDto);

            // Handle image assignment using ImageIds
            if (productDto.ImageIds != null && productDto.ImageIds.Count > 0)
            {
                // Verify that all image IDs exist and are available (not already assigned to another product)
                var images = await _imageService.GetImagesByIdsAsync(productDto.ImageIds);
                
                // Check if any images are already assigned to other products
                var unavailableImages = images.Where(img => img.ProductId.HasValue).ToList();
                if (unavailableImages.Any())
                {
                    var unavailableIds = string.Join(", ", unavailableImages.Select(img => img.Id));
                    throw new InvalidOperationException($"The following images are already assigned to other products: {unavailableIds}");
                }
                
                product.Images = images;
            }

            // Create the product
            await _unitOfWork.Products.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            // Associate images with the product
            if (productDto.ImageIds != null && productDto.ImageIds.Count > 0 && product.Images != null)
            {
                foreach (var image in product.Images)
                {
                    image.ProductId = product.Id;
                }
                await _unitOfWork.SaveChangesAsync();
            }
            
            return product;
        }        public async Task UpdateAsync(int id, UpdateProductDTO productDto)
        {
            // Verify category exists
            _ = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            // Get product or throw if not found
            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // Map basic product properties (excluding images)
            _mapper.Map(productDto, product);

            // Handle image associations: Remove old associations and create new ones
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // Step 1: Remove all existing image associations for this product
                    if (product.Images != null && product.Images.Any())
                    {
                        foreach (var existingImage in product.Images.ToList())
                        {
                            existingImage.ProductId = null; // Unassociate from this product
                        }
                        product.Images.Clear(); // Clear the collection
                    }

                    // Step 2: Associate new images if provided
                    if (productDto.ImageIds != null && productDto.ImageIds.Count > 0)
                    {
                        // Verify that all image IDs exist and are available
                        var newImages = await _imageService.GetImagesByIdsAsync(productDto.ImageIds);
                        
                        // Check if any images are already assigned to other products
                        var unavailableImages = newImages.Where(img => img.ProductId.HasValue && img.ProductId != id).ToList();
                        if (unavailableImages.Any())
                        {
                            var unavailableIds = string.Join(", ", unavailableImages.Select(img => img.Id));
                            throw new InvalidOperationException($"The following images are already assigned to other products: {unavailableIds}");
                        }
                        
                        // Associate new images with this product
                        foreach (var newImage in newImages)
                        {
                            newImage.ProductId = product.Id;
                            product.Images.Add(newImage);
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