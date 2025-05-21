using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/Customer
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers()
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
    }
}
