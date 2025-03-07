using System.Threading.Tasks;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<UserDTO> GetUserByIdAsync(int id);
    }
} 