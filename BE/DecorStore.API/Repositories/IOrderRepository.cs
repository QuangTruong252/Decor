using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetByIdWithItemsAsync(int id);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task UpdateStatusAsync(int id, string status);
        Task DeleteAsync(int id);
    }
} 