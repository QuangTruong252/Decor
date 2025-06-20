using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Services.Excel;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Models;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IProductExcelService _productExcelService;

        public ProductsController(IProductService productService, IProductExcelService productExcelService, ILogger<ProductsController> logger) 
            : base(logger)
        {
            _productService = productService;
            _productExcelService = productExcelService;
        }        // GET: api/Products
        [HttpGet]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm", "categoryId", "minPrice", "maxPrice", "sortBy", "isAscending", "isFeatured", "isActive" })]
        public async Task<ActionResult<PagedResult<ProductDTO>>> GetProducts([FromQuery] ProductFilterDTO filter)
        {
            var result = await _productService.GetPagedProductsAsync(filter);
            return HandlePagedResult(result);
        }        // GET: api/Products/all (for backward compatibility)
        [HttpGet("all")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
        {
            var result = await _productService.GetAllProductsAsync();
            return HandleResult(result);
        }        // GET: api/Products/5
        [HttpGet("{id}")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Language")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return HandleResult(result);
        }        // GET: api/Products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "count" })]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByCategory(int categoryId, [FromQuery] int count = 20)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryId, count);
            return HandleResult(result);
        }        // GET: api/Products/featured
        [HttpGet("featured")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "count" })]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFeaturedProducts([FromQuery] int count = 10)
        {
            var result = await _productService.GetFeaturedProductsAsync(count);
            return HandleResult(result);
        }        // GET: api/Products/{id}/related
        [HttpGet("{id}/related")]
        [ResponseCache(Duration = 1200, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "count" })]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetRelatedProducts(int id, [FromQuery] int count = 5)
        {
            var result = await _productService.GetRelatedProductsAsync(id, count);
            return HandleResult(result);
        }        // GET: api/Products/top-rated
        [HttpGet("top-rated")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "count" })]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetTopRatedProducts([FromQuery] int count = 10)
        {
            var result = await _productService.GetTopRatedProductsAsync(count);
            return HandleResult(result);
        }        // GET: api/Products/low-stock
        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")] // Only admin can view low stock products
        [ResponseCache(Duration = 180, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "threshold" })]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var result = await _productService.GetLowStockProductsAsync(threshold);
            return HandleResult(result);
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create products
        public async Task<ActionResult<ProductDTO>> CreateProduct(CreateProductDTO productDto)        {
            // Validate model state
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _productService.CreateAsync(productDto);
            return HandleCreateResult(result);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can update products
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDTO productDto)        {
            // Validate model state
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _productService.UpdateAsync(id, productDto);
            return HandleResult(result);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can delete products
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return HandleResult(result);
        }        // DELETE: api/Products/bulk
        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")] // Only admin can bulk delete products
        public async Task<IActionResult> BulkDeleteProducts(BulkDeleteDTO bulkDeleteDto)
        {
            // Validate model state
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            var result = await _productService.BulkDeleteProductsAsync(bulkDeleteDto);
            return HandleResult(result);
        }

        // POST: api/Products/{id}/images
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddImageToProduct(int id, IFormFile image)
        {
            var result = await _productService.AddImageToProductAsync(id, image);
            if (result.IsSuccess)
            {
                return Ok(new { message = "Image added successfully" });
            }
            return HandleResult(result);
        }

        // DELETE: api/Products/{productId}/images/{imageId}
        [HttpDelete("{productId}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveImageFromProduct(int productId, int imageId)
        {
            var result = await _productService.RemoveImageFromProductAsync(productId, imageId);
            if (result.IsSuccess)
            {
                return Ok(new { message = "Image removed successfully" });
            }
            return HandleResult(result);
        }

        #region Excel Import/Export Endpoints

        // POST: api/Products/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelImportResultDTO<ProductExcelDTO>>> ImportProducts(IFormFile file, [FromQuery] bool validateOnly = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only .xlsx files are supported");
                }

                using var stream = file.OpenReadStream();
                var result = await _productExcelService.ImportProductsAsync(stream, validateOnly);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Products/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportProducts([FromQuery] ProductFilterDTO? filter, [FromQuery] string? format = "xlsx")
        {
            try
            {
                var exportRequest = new ExcelExportRequestDTO
                {
                    WorksheetName = "Products Export",
                    IncludeFilters = true,
                    FreezeHeaderRow = true,
                    AutoFitColumns = true
                };

                var fileBytes = await _productExcelService.ExportProductsAsync(filter, exportRequest);
                var fileName = $"Products_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Products/export-template
        [HttpGet("export-template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductImportTemplate([FromQuery] bool includeExample = true)
        {
            try
            {
                var templateBytes = await _productExcelService.CreateProductTemplateAsync(includeExample);
                var fileName = $"Product_Import_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";

                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Products/validate-import
        [HttpPost("validate-import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelValidationResultDTO>> ValidateProductImport(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only .xlsx files are supported");
                }

                using var stream = file.OpenReadStream();
                var result = await _productExcelService.ValidateProductExcelAsync(stream);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Products/import-statistics
        [HttpPost("import-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductImportStatisticsDTO>> GetImportStatistics(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only .xlsx files are supported");
                }

                using var stream = file.OpenReadStream();
                var statistics = await _productExcelService.GetImportStatisticsAsync(stream);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion
    }
}
