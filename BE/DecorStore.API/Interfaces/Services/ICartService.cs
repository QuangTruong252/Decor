using DecorStore.API.DTOs;
using DecorStore.API.Common;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public interface ICartService
    {
        Task<Result<CartDTO>> GetCartAsync(int? userId, string? sessionId);
        Task<Result<CartDTO>> AddToCartAsync(int? userId, string? sessionId, AddToCartDTO addToCartDto);
        Task<Result<CartDTO>> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDTO updateCartItemDto);
        Task<Result<CartDTO>> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId);
        Task<Result<CartDTO>> ClearCartAsync(int? userId, string? sessionId);
        Task<Result> MergeCartsAsync(int userId, string sessionId);
    }
}
