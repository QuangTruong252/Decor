using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DecorStore.API.Controllers.Base;
using DecorStore.API.DTOs;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.Models;
using DecorStore.API.Services;
using DecorStore.API.Services.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderExcelService _orderExcelService;

        public OrderController([NotNull] IOrderService orderService, 
                             [NotNull] IOrderExcelService orderExcelService, 
                             [NotNull] ILogger<OrderController> logger)
            : base(logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _orderExcelService = orderExcelService ?? throw new ArgumentNullException(nameof(orderExcelService));
        }

        // GET: api/Order
        [HttpGet]
        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Gets a paged list of orders
        /// </summary>
        /// <param name="filter">Filter criteria for orders</param>
        /// <returns>Paged list of orders</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<OrderDTO>>> GetOrders([FromQuery] OrderFilterDTO? filter)
        {
            var result = await _orderService.GetPagedOrdersAsync(filter ?? new OrderFilterDTO());
            return HandlePagedResult(result);
        }

        // GET: api/Order/all (for backward compatibility)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return HandleResult(result);
        }

        // GET: api/Order/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByUser(int userId)
        {
            if (!await CanAccessUserData(userId))
            {
                return Forbid();
            }

            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            return HandleResult(result);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                return HandleResult(result);
            }

            if (!await CanAccessUserData(result.Data.UserId))
            {
                return Forbid();
            }

            return HandleResult(result);
        }

        // POST: api/Order
        [HttpPost]
        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="orderDto">Order creation data</param>
        /// <returns>Created order details</returns>
        [HttpPost]
        public async Task<ActionResult<OrderDTO>> CreateOrder([FromBody][Required] CreateOrderDTO orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest("Order data is required");
            }

            orderDto.UserId = int.Parse(GetCurrentUserId() ?? "0");

            var result = await _orderService.CreateOrderAsync(orderDto);
            
            if (result.IsSuccess)
            {
                return HandleCreateResult(result, nameof(GetOrder), new { id = result.Data.Id });
            }
            
            return HandleResult(result);
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDTO>> UpdateOrder(int id, UpdateOrderDTO orderDto)
        {
            var result = await _orderService.UpdateOrderAsync(id, orderDto);
            return HandleResult(result);
        }

        // PUT: api/Order/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDTO>> UpdateOrderStatus(int id, UpdateOrderStatusDTO statusDto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, statusDto);
            return HandleResult(result);
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteOrder(int id)
        {            // First get the order to check ownership
            var orderResult = await _orderService.GetOrderByIdAsync(id);
            if (!orderResult.IsSuccess)
            {
                return BadRequest(orderResult.Error);
            }

            if (!await CanAccessUserData(orderResult.Data!.UserId))
            {
                return Forbid();
            }

            var result = await _orderService.DeleteOrderAsync(id);
            return HandleResult(result);
        }

        // DELETE: api/Order/bulk
        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")] // Only admin can bulk delete orders
        public async Task<ActionResult<bool>> BulkDeleteOrders(BulkDeleteDTO bulkDeleteDto)
        {
            var result = await _orderService.BulkDeleteOrdersAsync(bulkDeleteDto);
            return HandleResult(result);
        }

        // GET: api/Order/recent
        [HttpGet("recent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetRecentOrders([FromQuery] int count = 10)
        {
            var result = await _orderService.GetRecentOrdersAsync(count);
            return HandleResult(result);
        }

        // GET: api/Order/status/{status}
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByStatus(string status, [FromQuery] int count = 50)
        {
            var result = await _orderService.GetOrdersByStatusAsync(status, count);
            return HandleResult(result);
        }

        // GET: api/Order/date-range
        [HttpGet("date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return HandleResult(result);
        }

        // GET: api/Order/revenue
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<decimal>> GetTotalRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _orderService.GetTotalRevenueAsync(startDate, endDate);
            return HandleResult(result);
        }

        // GET: api/Order/status-counts
        [HttpGet("status-counts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Dictionary<string, int>>> GetOrderStatusCounts()
        {
            var result = await _orderService.GetOrderStatusCountsAsync();
            return HandleResult(result);
        }

        #region Excel Import/Export Endpoints

        // POST: api/Order/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelImportResultDTO<OrderExcelDTO>>> ImportOrders(IFormFile file, [FromQuery] bool validateOnly = false)
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
                return Ok(result.Data);
            }

            // Return the validation errors and summary
            return BadRequest(new
            {
                Summary = result.Summary,
                Errors = result.Errors,
                TotalRows = result.TotalRows,
                SuccessfulRows = result.SuccessfulRows,
                ErrorRows = result.ErrorRows
            });
        }

        // GET: api/Order/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<byte[]>> ExportOrders([FromQuery] OrderFilterDTO? filter, [FromQuery] string? format = "xlsx")
        {
            var exportRequest = new ExcelExportRequestDTO
            {
                WorksheetName = "Orders Export",
                IncludeFilters = true,
                FreezeHeaderRow = true,
                AutoFitColumns = true
            };

            var result = await _orderExcelService.ExportOrdersAsync(filter, exportRequest);
            if (result.IsSuccess)
            {
                var fileName = $"Orders_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            return BadRequest(result.Error);
        }

        // GET: api/Order/export-template
        [HttpGet("export-template")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<byte[]>> GetOrderImportTemplate([FromQuery] bool includeExample = true)
        {
            var result = await _orderExcelService.CreateOrderTemplateAsync(includeExample);
            if (result.IsSuccess)
            {
                var fileName = $"Order_Import_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            return BadRequest(result.Error);
        }

        // POST: api/Order/validate-import
        [HttpPost("validate-import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExcelValidationResultDTO>> ValidateOrderImport(IFormFile file)
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
            return result.IsValid ? Ok(result) : BadRequest(result);
        }

        // POST: api/Order/import-statistics
        [HttpPost("import-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderImportStatisticsDTO>> GetImportStatistics(IFormFile file)
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
            var result = await _orderExcelService.GetImportStatisticsAsync(stream);
            return Ok(result);
        }

        #endregion

        #region Helper Methods

        private async Task<bool> CanAccessUserData(int userId)
        {
            var currentUserId = int.Parse(GetCurrentUserId() ?? "0");
            return currentUserId == userId || HasRole("Admin");
        }

        #endregion
    }
}
