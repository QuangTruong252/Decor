using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using DecorStore.API.Controllers.Base;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerExcelService _customerExcelService;

        public CustomerController(ICustomerService customerService, ICustomerExcelService customerExcelService, ILogger<CustomerController> logger)
            : base(logger)
        {
            _customerService = customerService;
            _customerExcelService = customerExcelService;
        }

        // GET: api/Customer
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<CustomerDTO>>> GetCustomers([FromQuery] CustomerFilterDTO filter)
        {
            var result = await _customerService.GetPagedCustomersAsync(filter);
            return HandlePagedResult(result);
        }

        // GET: api/Customer/all (for backward compatibility)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            var result = await _customerService.GetAllCustomersAsync();
            return HandleResult(result);
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            return HandleResult(result);
        }

        // GET: api/Customer/email/{email}
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> GetCustomerByEmail(string email)
        {
            var result = await _customerService.GetCustomerByEmailAsync(email);
            return HandleResult(result);
        }

        // POST: api/Customer
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> CreateCustomer(CreateCustomerDTO customerDto)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualCustomerDto = await TryManualDeserializationAsync(customerDto, _logger);

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _customerService.CreateCustomerAsync(actualCustomerDto);
            return HandleCreateResult(result);
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> UpdateCustomer(int id, UpdateCustomerDTO customerDto)
        {
            // WORKAROUND: ASP.NET Core model binding is broken, so manually deserialize the JSON
            var actualCustomerDto = await TryManualDeserializationAsync(customerDto, _logger);

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            var result = await _customerService.UpdateCustomerAsync(id, actualCustomerDto);
            return HandleResult(result);
        }

        // DELETE: api/Customer/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            return HandleDeleteResult(result);
        }

        // GET: api/Customer/with-orders
        [HttpGet("with-orders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersWithOrders()
        {
            var result = await _customerService.GetCustomersWithOrdersAsync();
            return HandleResult(result);
        }

        // GET: api/Customer/top-by-order-count
        [HttpGet("top-by-order-count")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetTopCustomersByOrderCount([FromQuery] int count = 10)
        {
            var result = await _customerService.GetTopCustomersByOrderCountAsync(count);
            return HandleResult(result);
        }

        // GET: api/Customer/top-by-spending
        [HttpGet("top-by-spending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetTopCustomersBySpending([FromQuery] int count = 10)
        {
            var result = await _customerService.GetTopCustomersBySpendingAsync(count);
            return HandleResult(result);
        }

        // GET: api/Customer/{customerId}/order-count
        [HttpGet("{customerId}/order-count")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> GetOrderCountByCustomer(int customerId)
        {
            var result = await _customerService.GetOrderCountByCustomerAsync(customerId);
            return HandleResult(result);
        }

        // GET: api/Customer/{customerId}/total-spent
        [HttpGet("{customerId}/total-spent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetTotalSpentByCustomer(int customerId)
        {
            var result = await _customerService.GetTotalSpentByCustomerAsync(customerId);
            return HandleResult(result);
        }

        // GET: api/Customer/by-location
        [HttpGet("by-location")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersByLocation(
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] string? country = null)
        {
            var result = await _customerService.GetCustomersByLocationAsync(city, state, country);
            return HandleResult(result);
        }

        #region Excel Import/Export Endpoints

        // POST: api/Customer/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelImportResultDTO<CustomerExcelDTO>>> ImportCustomers(IFormFile file, [FromQuery] bool validateOnly = false)
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
                var result = await _customerExcelService.ImportCustomersAsync(stream, validateOnly);

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

        // GET: api/Customer/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCustomers([FromQuery] CustomerFilterDTO? filter, [FromQuery] string? format = "xlsx")
        {
            try
            {
                var exportRequest = new ExcelExportRequestDTO
                {
                    WorksheetName = "Customers Export",
                    IncludeFilters = true,
                    FreezeHeaderRow = true,
                    AutoFitColumns = true
                };

                var fileBytes = await _customerExcelService.ExportCustomersAsync(filter, exportRequest);
                var fileName = $"Customers_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Customer/export-template
        [HttpGet("export-template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCustomerImportTemplate([FromQuery] bool includeExample = true)
        {
            try
            {
                var templateBytes = await _customerExcelService.CreateCustomerTemplateAsync(includeExample);
                var fileName = $"Customer_Import_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";

                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Customer/validate-import
        [HttpPost("validate-import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelValidationResultDTO>> ValidateCustomerImport(IFormFile file)
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
                var result = await _customerExcelService.ValidateCustomerExcelAsync(stream);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Customer/import-statistics
        [HttpPost("import-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerImportStatisticsDTO>> GetImportStatistics(IFormFile file)
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
                var statistics = await _customerExcelService.GetImportStatisticsAsync(stream);

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
