using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryExcelService _categoryExcelService;

        public CategoryController(ICategoryService categoryService, ICategoryExcelService categoryExcelService)
        {
            _categoryService = categoryService;
            _categoryExcelService = categoryExcelService;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<PagedResult<CategoryDTO>>> GetCategories([FromQuery] CategoryFilterDTO filter)
        {
            var pagedCategories = await _categoryService.GetPagedCategoriesAsync(filter);
            return Ok(pagedCategories);
        }

        // GET: api/Category/all (for backward compatibility)
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/Category/hierarchical
        [HttpGet("hierarchical")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetHierarchicalCategories()
        {
            var categories = await _categoryService.GetHierarchicalCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // GET: api/Category/slug/home-decor
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // POST: api/Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Category>> CreateCategory([FromForm] CreateCategoryDTO categoryDto)
        {
            try
            {
                var category = await _categoryService.CreateAsync(categoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating category: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { message = "An error occurred while creating the category. Please try again." });
            }
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] UpdateCategoryDTO categoryDto)
        {
            try
            {
                await _categoryService.UpdateAsync(id, categoryDto);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating category: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { message = "An error occurred while updating the category. Please try again." });
            }
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Category/with-product-count
        [HttpGet("with-product-count")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesWithProductCount()
        {
            var categories = await _categoryService.GetCategoriesWithProductCountAsync();
            return Ok(categories);
        }

        // GET: api/Category/{parentId}/subcategories
        [HttpGet("{parentId}/subcategories")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetSubcategories(int parentId)
        {
            var subcategories = await _categoryService.GetSubcategoriesAsync(parentId);
            return Ok(subcategories);
        }

        // GET: api/Category/{categoryId}/product-count
        [HttpGet("{categoryId}/product-count")]
        public async Task<ActionResult<int>> GetProductCountByCategory(int categoryId)
        {
            var count = await _categoryService.GetProductCountByCategoryAsync(categoryId);
            return Ok(count);
        }

        // GET: api/Category/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetPopularCategories([FromQuery] int count = 10)
        {
            var categories = await _categoryService.GetPopularCategoriesAsync(count);
            return Ok(categories);
        }

        // GET: api/Category/root
        [HttpGet("root")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetRootCategories()
        {
            var categories = await _categoryService.GetRootCategoriesAsync();
            return Ok(categories);
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