using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
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

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Order
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
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
    }
}