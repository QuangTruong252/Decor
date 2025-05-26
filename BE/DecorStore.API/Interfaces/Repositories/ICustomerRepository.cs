using DecorStore.API.Models;
using DecorStore.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public interface ICustomerRepository
    {
        // Paginated queries
        Task<PagedResult<Customer>> GetPagedAsync(CustomerFilterDTO filter);
        Task<IEnumerable<Customer>> GetAllAsync();

        // Single item queries
        Task<Customer> GetByIdAsync(int id);
        Task<Customer> GetByEmailAsync(string email);

        // Count and existence checks
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<int> GetTotalCountAsync(CustomerFilterDTO filter);

        // CRUD operations
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);

        // Advanced queries
        Task<IEnumerable<Customer>> GetCustomersWithOrdersAsync();
        Task<IEnumerable<Customer>> GetTopCustomersByOrderCountAsync(int count = 10);
        Task<IEnumerable<Customer>> GetTopCustomersBySpendingAsync(int count = 10);
        Task<int> GetOrderCountByCustomerAsync(int customerId);
        Task<decimal> GetTotalSpentByCustomerAsync(int customerId);
        Task<IEnumerable<Customer>> GetCustomersByLocationAsync(string? city = null, string? state = null, string? country = null);
    }
}
