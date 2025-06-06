using System.Threading.Tasks;
using DecorStore.API.DTOs;
using DecorStore.API.Common;

namespace DecorStore.API.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDTO>> RegisterAsync(RegisterDTO registerDto);
        Task<Result<AuthResponseDTO>> LoginAsync(LoginDTO loginDto);
        Task<Result<UserDTO>> GetUserByIdAsync(int id);
        Task<Result<UserDTO>> MakeAdminAsync(string email);
        Task<Result> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto);
        Task<Result<AuthResponseDTO>> RefreshTokenAsync(string token);
    }
}
