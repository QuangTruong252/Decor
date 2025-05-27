using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderExcelService _orderExcelService;

        public OrderController(IOrderService orderService, IOrderExcelService orderExcelService)
        {
            _orderService = orderService;
            _orderExcelService = orderExcelService;
        }

        // GET: api/Order
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<OrderDTO>>> GetOrders([FromQuery] OrderFilterDTO filter)
        {
            var pagedOrders = await _orderService.GetPagedOrdersAsync(filter);
            return Ok(pagedOrders);
        }

        // GET: api/Order/all (for backward compatibility)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/Order/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByUser(int userId)
        {
            // Kiểm tra quyền: chỉ admin hoặc chính user đó mới được xem order của user
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền: chỉ admin hoặc chính user đó mới được xem chi tiết order
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != order.UserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return order;
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<OrderDTO>> CreateOrder(CreateOrderDTO orderDto)
        {
            try
            {
                // Gán UserId từ token vào orderDto
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                orderDto.UserId = currentUserId;

                var createdOrderDto = await _orderService.CreateOrderAsync(orderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = createdOrderDto.Id }, createdOrderDto);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDTO orderDto)
        {
            try
            {
                await _orderService.UpdateOrderAsync(id, orderDto);
                return NoContent();
            }
            catch (System.Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Order/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDTO statusDto)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, statusDto);
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

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // Kiểm tra order thuộc về user hiện tại hoặc là admin
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId != order.UserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _orderService.DeleteOrderAsync(id);
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

        // DELETE: api/Order/bulk
        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")] // Only admin can bulk delete orders
        public async Task<IActionResult> BulkDeleteOrders(BulkDeleteDTO bulkDeleteDto)
        {
            try
            {
                await _orderService.BulkDeleteOrdersAsync(bulkDeleteDto);
                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/recent
        [HttpGet("recent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetRecentOrders([FromQuery] int count = 10)
        {
            var orders = await _orderService.GetRecentOrdersAsync(count);
            return Ok(orders);
        }

        // GET: api/Order/status/{status}
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByStatus(string status, [FromQuery] int count = 50)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status, count);
            return Ok(orders);
        }

        // GET: api/Order/date-range
        [HttpGet("date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(orders);
        }

        // GET: api/Order/revenue
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetTotalRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var revenue = await _orderService.GetTotalRevenueAsync(startDate, endDate);
            return Ok(revenue);
        }

        // GET: api/Order/status-counts
        [HttpGet("status-counts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Dictionary<string, int>>> GetOrderStatusCounts()
        {
            var statusCounts = await _orderService.GetOrderStatusCountsAsync();
            return Ok(statusCounts);
        }

        #region Excel Import/Export Endpoints

        // POST: api/Order/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelImportResultDTO<OrderExcelDTO>>> ImportOrders(IFormFile file, [FromQuery] bool validateOnly = false)
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
                var result = await _orderExcelService.ImportOrdersAsync(stream, validateOnly);

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

        // GET: api/Order/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportOrders([FromQuery] OrderFilterDTO? filter, [FromQuery] string? format = "xlsx")
        {
            try
            {
                var exportRequest = new ExcelExportRequestDTO
                {
                    WorksheetName = "Orders Export",
                    IncludeFilters = true,
                    FreezeHeaderRow = true,
                    AutoFitColumns = true
                };

                var fileBytes = await _orderExcelService.ExportOrdersAsync(filter, exportRequest);
                var fileName = $"Orders_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Order/export-template
        [HttpGet("export-template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderImportTemplate([FromQuery] bool includeExample = true)
        {
            try
            {
                var templateBytes = await _orderExcelService.CreateOrderTemplateAsync(includeExample);
                var fileName = $"Order_Import_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";

                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Order/validate-import
        [HttpPost("validate-import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelValidationResultDTO>> ValidateOrderImport(IFormFile file)
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
                var result = await _orderExcelService.ValidateOrderExcelAsync(stream);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Order/import-statistics
        [HttpPost("import-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderImportStatisticsDTO>> GetImportStatistics(IFormFile file)
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
                var statistics = await _orderExcelService.GetImportStatisticsAsync(stream);

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