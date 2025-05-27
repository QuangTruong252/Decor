using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerExcelService _customerExcelService;

        public CustomerController(ICustomerService customerService, ICustomerExcelService customerExcelService)
        {
            _customerService = customerService;
            _customerExcelService = customerExcelService;
        }

        // GET: api/Customer
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<CustomerDTO>>> GetCustomers([FromQuery] CustomerFilterDTO filter)
        {
            var pagedCustomers = await _customerService.GetPagedCustomersAsync(filter);
            return Ok(pagedCustomers);
        }

        // GET: api/Customer/all (for backward compatibility)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Customer/email/{email}
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CustomerDTO>> GetCustomerByEmail(string email)
        {
            try
            {
                var customer = await _customerService.GetCustomerByEmailAsync(email);
                return Ok(customer);
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Customer
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Customer>> CreateCustomer(CreateCustomerDTO customerDto)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(customerDto);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating customer: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { message = "An error occurred while creating the customer. Please try again." });
            }
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDTO customerDto)
        {
            try
            {
                await _customerService.UpdateCustomerAsync(id, customerDto);
                return NoContent();
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while updating the customer: {ex.Message}" });
            }
        }

        // DELETE: api/Customer/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (DecorStore.API.Exceptions.NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while deleting the customer: {ex.Message}" });
            }
        }

        // GET: api/Customer/with-orders
        [HttpGet("with-orders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersWithOrders()
        {
            var customers = await _customerService.GetCustomersWithOrdersAsync();
            return Ok(customers);
        }

        // GET: api/Customer/top-by-order-count
        [HttpGet("top-by-order-count")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetTopCustomersByOrderCount([FromQuery] int count = 10)
        {
            var customers = await _customerService.GetTopCustomersByOrderCountAsync(count);
            return Ok(customers);
        }

        // GET: api/Customer/top-by-spending
        [HttpGet("top-by-spending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetTopCustomersBySpending([FromQuery] int count = 10)
        {
            var customers = await _customerService.GetTopCustomersBySpendingAsync(count);
            return Ok(customers);
        }

        // GET: api/Customer/{customerId}/order-count
        [HttpGet("{customerId}/order-count")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> GetOrderCountByCustomer(int customerId)
        {
            var count = await _customerService.GetOrderCountByCustomerAsync(customerId);
            return Ok(count);
        }

        // GET: api/Customer/{customerId}/total-spent
        [HttpGet("{customerId}/total-spent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetTotalSpentByCustomer(int customerId)
        {
            var totalSpent = await _customerService.GetTotalSpentByCustomerAsync(customerId);
            return Ok(totalSpent);
        }

        // GET: api/Customer/by-location
        [HttpGet("by-location")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersByLocation(
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] string? country = null)
        {
            var customers = await _customerService.GetCustomersByLocationAsync(city, state, country);
            return Ok(customers);
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
