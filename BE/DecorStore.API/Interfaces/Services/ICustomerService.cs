using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Models;

namespace DecorStore.API.Services
{
    public interface ICustomerService
    {
        // Pagination methods
        Task<PagedResult<CustomerDTO>> GetPagedCustomersAsync(CustomerFilterDTO filter);
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();

        // Single item queries
        Task<CustomerDTO> GetCustomerByIdAsync(int id);
        Task<CustomerDTO> GetCustomerByEmailAsync(string email);

        // CRUD operations
        Task<Customer> CreateCustomerAsync(CreateCustomerDTO customerDto);
        Task UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto);
        Task DeleteCustomerAsync(int id);

        // Advanced queries
        Task<IEnumerable<CustomerDTO>> GetCustomersWithOrdersAsync();
        Task<IEnumerable<CustomerDTO>> GetTopCustomersByOrderCountAsync(int count = 10);
        Task<IEnumerable<CustomerDTO>> GetTopCustomersBySpendingAsync(int count = 10);
        Task<int> GetOrderCountByCustomerAsync(int customerId);
        Task<decimal> GetTotalSpentByCustomerAsync(int customerId);
        Task<IEnumerable<CustomerDTO>> GetCustomersByLocationAsync(string? city = null, string? state = null, string? country = null);
    }
}
