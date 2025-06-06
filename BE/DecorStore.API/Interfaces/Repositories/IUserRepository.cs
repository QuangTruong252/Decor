using DecorStore.API.Models;
using DecorStore.API.DTOs;
using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // Basic CRUD operations
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        
        // Authentication specific
        Task<User> ValidateUserAsync(string email, string password);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
    }
}
