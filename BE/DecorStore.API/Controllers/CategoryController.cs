using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using DecorStore.API.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryExcelService _categoryExcelService;

        public CategoryController(ICategoryService categoryService, ICategoryExcelService categoryExcelService, ILogger<CategoryController> logger)
            : base(logger)
        {
            _categoryService = categoryService;
            _categoryExcelService = categoryExcelService;
        }        // GET: api/Category
        [HttpGet]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm", "parentId", "sortBy", "isAscending" })]
        public async Task<ActionResult<PagedResult<CategoryDTO>>> GetCategories([FromQuery] CategoryFilterDTO filter)
        {
            var result = await _categoryService.GetPagedCategoriesAsync(filter);
            return HandlePagedResult(result);
        }        // GET: api/Category/all (for backward compatibility)
        [HttpGet("all")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return HandleResult(result);
        }        // GET: api/Category/hierarchical
        [HttpGet("hierarchical")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetHierarchicalCategories()
        {
            var result = await _categoryService.GetHierarchicalCategoriesAsync();
            return HandleResult(result);
        }        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return HandleResult(result);
        }        // GET: api/Category/slug/home-decor
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryBySlug(string slug)
        {
            var result = await _categoryService.GetCategoryBySlugAsync(slug);
            return HandleResult(result);
        }        // POST: api/Category
        [HttpPost]
        [Authorize(Roles = "Admin")]        public async Task<ActionResult<CategoryDTO>> CreateCategory(CreateCategoryDTO categoryDto)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _categoryService.CreateAsync(categoryDto);
            return HandleCreateResult(result);
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDTO categoryDto)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualCategoryDto = await TryManualDeserializationAsync(categoryDto, _logger);

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _categoryService.UpdateAsync(id, actualCategoryDto);
            return HandleResult(result);
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return HandleDeleteResult(result);
        }        // GET: api/Category/with-product-count
        [HttpGet("with-product-count")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesWithProductCount()
        {
            var result = await _categoryService.GetCategoriesWithProductCountAsync();
            return HandleResult(result);
        }        // GET: api/Category/{parentId}/subcategories
        [HttpGet("{parentId}/subcategories")]
        [ResponseCache(Duration = 1200, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetSubcategories(int parentId)
        {
            var result = await _categoryService.GetSubcategoriesAsync(parentId);
            return HandleResult(result);
        }        // GET: api/Category/{categoryId}/product-count
        [HttpGet("{categoryId}/product-count")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<int>> GetProductCountByCategory(int categoryId)
        {
            var result = await _categoryService.GetProductCountByCategoryAsync(categoryId);
            return HandleResult(result);
        }        // GET: api/Category/popular
        [HttpGet("popular")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "count" })]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetPopularCategories([FromQuery] int count = 10)
        {
            var result = await _categoryService.GetPopularCategoriesAsync(count);
            return HandleResult(result);
        }        // GET: api/Category/root
        [HttpGet("root")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetRootCategories()
        {
            var result = await _categoryService.GetRootCategoriesAsync();
            return HandleResult(result);
        }

        #region Excel Import/Export Endpoints

        // POST: api/Category/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelImportResultDTO<CategoryExcelDTO>>> ImportCategories(IFormFile file, [FromQuery] bool validateOnly = false)
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
                var result = await _categoryExcelService.ImportCategoriesAsync(stream, validateOnly);

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

        // GET: api/Category/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCategories([FromQuery] CategoryFilterDTO? filter, [FromQuery] string? format = "xlsx")
        {
            try
            {
                var exportRequest = new ExcelExportRequestDTO
                {
                    WorksheetName = "Categories Export",
                    IncludeFilters = true,
                    FreezeHeaderRow = true,
                    AutoFitColumns = true
                };

                var fileBytes = await _categoryExcelService.ExportCategoriesAsync(filter, exportRequest);
                var fileName = $"Categories_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Category/export-template
        [HttpGet("export-template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryImportTemplate([FromQuery] bool includeExample = true)
        {
            try
            {
                var templateBytes = await _categoryExcelService.CreateCategoryTemplateAsync(includeExample);
                var fileName = $"Category_Import_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";

                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Category/validate-import
        [HttpPost("validate-import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelValidationResultDTO>> ValidateCategoryImport(IFormFile file)
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
                var result = await _categoryExcelService.ValidateCategoryExcelAsync(stream);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Category/import-statistics
        [HttpPost("import-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryImportStatisticsDTO>> GetImportStatistics(IFormFile file)
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
                var statistics = await _categoryExcelService.GetImportStatisticsAsync(stream);

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
