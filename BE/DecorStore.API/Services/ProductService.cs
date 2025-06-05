using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Common;
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

        public async Task<Result<PagedResult<ProductDTO>>> GetPagedProductsAsync(ProductFilterDTO filter)
        {
            try
            {
                if (filter == null)
                {
                    return Result<PagedResult<ProductDTO>>.Failure("Filter cannot be null", "INVALID_INPUT");
                }

                var pagedProducts = await _unitOfWork.Products.GetPagedAsync(filter);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(pagedProducts.Items);

                var result = new PagedResult<ProductDTO>(productDtos, pagedProducts.Pagination.TotalCount,
                    pagedProducts.Pagination.CurrentPage, pagedProducts.Pagination.PageSize);

                return Result<PagedResult<ProductDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PagedResult<ProductDTO>>.Failure($"Failed to get paginated products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetAllAsync(ProductFilterDTO filter)
        {
            try
            {
                if (filter == null)
                {
                    return Result<IEnumerable<Product>>.Failure("Filter cannot be null", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetAllAsync(filter);
                return Result<IEnumerable<Product>>.Success(products);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<Product>>.Failure($"Failed to get products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<ProductDTO>> CreateAsync(CreateProductDTO productDto)
        {
            try
            {
                // Input validation
                if (productDto == null)
                {
                    return Result<ProductDTO>.Failure("Product data cannot be null", "INVALID_INPUT");
                }

                // Verify category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
                if (category == null)
                {
                    return Result<ProductDTO>.NotFound("Category");
                }

                // Validate unique constraints
                if (await _unitOfWork.Products.SkuExistsAsync(productDto.SKU))
                {
                    return Result<ProductDTO>.Failure("SKU already exists", "DUPLICATE_SKU");
                }

                if (await _unitOfWork.Products.SlugExistsAsync(productDto.Slug))
                {
                    return Result<ProductDTO>.Failure("Slug already exists", "DUPLICATE_SLUG");
                }

                // Map DTO to entity
                var product = _mapper.Map<Product>(productDto);

                // Create the product first
                await _unitOfWork.Products.AddAsync(product);
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
                        return Result<ProductDTO>.NotFound($"The following image IDs were not found: {string.Join(", ", missingIds)}");
                    }

                    // Associate images with the product using junction table
                    foreach (var imageId in productDto.ImageIds)
                    {
                        await _unitOfWork.Images.AddProductImageAsync(imageId, product.Id);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
                
                // Return the created product as DTO
                var createdProductDto = _mapper.Map<ProductDTO>(product);
                return Result<ProductDTO>.Success(createdProductDto);
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.Failure($"Failed to create product: {ex.Message}", "CREATE_ERROR");
            }
        }

        public async Task<Result> UpdateAsync(int id, UpdateProductDTO productDto)
        {
            try
            {
                // Input validation
                if (productDto == null)
                {
                    return Result.Failure("Product data cannot be null", "INVALID_INPUT");
                }

                if (id <= 0)
                {
                    return Result.Failure("Invalid product ID", "INVALID_INPUT");
                }

                // Verify category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(productDto.CategoryId);
                if (category == null)
                {
                    return Result.NotFound("Category");
                }

                // Get product or return not found
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Result.NotFound("Product");
                }

                // Validate unique constraints if fields are being changed
                if (product.SKU != productDto.SKU && await _unitOfWork.Products.SkuExistsAsync(productDto.SKU, id))
                {
                    return Result.Failure("SKU already exists", "DUPLICATE_SKU");
                }

                if (product.Slug != productDto.Slug && await _unitOfWork.Products.SlugExistsAsync(productDto.Slug, id))
                {
                    return Result.Failure("Slug already exists", "DUPLICATE_SLUG");
                }

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

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update product: {ex.Message}", "UPDATE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ProductDTO>>> GetAllProductsAsync()
        {
            try
            {
                var filter = new ProductFilterDTO { PageNumber = 1, PageSize = 100 };
                var products = await _unitOfWork.Products.GetAllAsync(filter);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get all products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<ProductDTO>> GetProductByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<ProductDTO>.Failure("Invalid product ID", "INVALID_INPUT");
                }

                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Result<ProductDTO>.NotFound("Product");
                }

                var productDto = _mapper.Map<ProductDTO>(product);
                return Result<ProductDTO>.Success(productDto);
            }
            catch (Exception ex)
            {
                return Result<ProductDTO>.Failure($"Failed to get product: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result> DeleteProductAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.Failure("Invalid product ID", "INVALID_INPUT");
                }

                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return Result.NotFound("Product");
                }

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
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to delete product: {ex.Message}", "DELETE_ERROR");
            }
        }

        public async Task<Result> BulkDeleteProductsAsync(BulkDeleteDTO bulkDeleteDto)
        {
            try
            {
                if (bulkDeleteDto?.Ids == null || !bulkDeleteDto.Ids.Any())
                {
                    return Result.Failure("No product IDs provided for deletion", "INVALID_INPUT");
                }

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

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to bulk delete products: {ex.Message}", "BULK_DELETE_ERROR");
            }
        }

        public async Task<Result> AddImageToProductAsync(int productId, IFormFile image)
        {
            try
            {
                if (productId <= 0)
                {
                    return Result.Failure("Invalid product ID", "INVALID_INPUT");
                }

                if (image == null || image.Length == 0)
                {
                    return Result.Failure("Image file cannot be null or empty", "INVALID_INPUT");
                }

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return Result.NotFound("Product");
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
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to add image to product: {ex.Message}", "IMAGE_ADD_ERROR");
            }
        }

        public async Task<Result> RemoveImageFromProductAsync(int productId, int imageId)
        {
            try
            {
                if (productId <= 0)
                {
                    return Result.Failure("Invalid product ID", "INVALID_INPUT");
                }

                if (imageId <= 0)
                {
                    return Result.Failure("Invalid image ID", "INVALID_INPUT");
                }

                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return Result.NotFound("Product");
                }

                // Check if the image is associated with this product
                var productImages = await _unitOfWork.Images.GetProductImagesByProductIdAsync(productId);
                var productImage = productImages.FirstOrDefault(pi => pi.ImageId == imageId);
                
                if (productImage == null)
                {
                    return Result.NotFound("Image not associated with this product");
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
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to remove image from product: {ex.Message}", "IMAGE_REMOVE_ERROR");
            }
        }

        public IEnumerable<ProductDTO> MapToProductDTOs(IEnumerable<Product> products)
        {
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        // Advanced query methods
        public async Task<Result<IEnumerable<ProductDTO>>> GetFeaturedProductsAsync(int count = 10)
        {
            try
            {
                if (count <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get featured products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ProductDTO>>> GetProductsByCategoryAsync(int categoryId, int count = 20)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Invalid category ID", "INVALID_INPUT");
                }

                if (count <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId, count);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get products by category: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ProductDTO>>> GetRelatedProductsAsync(int productId, int count = 5)
        {
            try
            {
                if (productId <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Invalid product ID", "INVALID_INPUT");
                }

                if (count <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetRelatedProductsAsync(productId, count);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get related products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ProductDTO>>> GetTopRatedProductsAsync(int count = 10)
        {
            try
            {
                if (count <= 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetTopRatedProductsAsync(count);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get top rated products: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ProductDTO>>> GetLowStockProductsAsync(int threshold = 10)
        {
            try
            {
                if (threshold < 0)
                {
                    return Result<IEnumerable<ProductDTO>>.Failure("Threshold must be non-negative", "INVALID_INPUT");
                }

                var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Result<IEnumerable<ProductDTO>>.Success(productDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ProductDTO>>.Failure($"Failed to get low stock products: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
