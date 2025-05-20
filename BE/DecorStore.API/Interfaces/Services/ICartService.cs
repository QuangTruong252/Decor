using DecorStore.API.DTOs;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public interface ICartService
    {
        Task<CartDTO> GetCartAsync(int? userId, string? sessionId);
        Task<CartDTO> AddToCartAsync(int? userId, string? sessionId, AddToCartDTO addToCartDto);
        Task<CartDTO> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDTO updateCartItemDto);
        Task<CartDTO> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId);
        Task<CartDTO> ClearCartAsync(int? userId, string? sessionId);
        Task MergeCartsAsync(int userId, string sessionId);
    }
}
