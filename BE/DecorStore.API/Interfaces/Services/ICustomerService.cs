using System.Collections.Generic;
using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface ICustomerService
    {
        // Pagination methods
        Task<Result<PagedResult<CustomerDTO>>> GetPagedCustomersAsync(CustomerFilterDTO filter);
        Task<Result<IEnumerable<CustomerDTO>>> GetAllCustomersAsync();

        // Single item queries
        Task<Result<CustomerDTO>> GetCustomerByIdAsync(int id);
        Task<Result<CustomerDTO>> GetCustomerByEmailAsync(string email);

        // CRUD operations
        Task<Result<CustomerDTO>> CreateCustomerAsync(CreateCustomerDTO customerDto);
        Task<Result<CustomerDTO>> UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto);
        Task<Result> DeleteCustomerAsync(int id);

        // Advanced queries
        Task<Result<IEnumerable<CustomerDTO>>> GetCustomersWithOrdersAsync();
        Task<Result<IEnumerable<CustomerDTO>>> GetTopCustomersByOrderCountAsync(int count = 10);
        Task<Result<IEnumerable<CustomerDTO>>> GetTopCustomersBySpendingAsync(int count = 10);
        Task<Result<int>> GetOrderCountByCustomerAsync(int customerId);
        Task<Result<decimal>> GetTotalSpentByCustomerAsync(int customerId);
        Task<Result<IEnumerable<CustomerDTO>>> GetCustomersByLocationAsync(string? city = null, string? state = null, string? country = null);
    }
}
