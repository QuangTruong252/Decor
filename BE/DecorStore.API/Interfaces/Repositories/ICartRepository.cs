using DecorStore.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecorStore.API.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart?> GetBySessionIdAsync(string sessionId);
        Task<Cart?> GetByIdAsync(int id);
        Task<Cart?> GetByIdWithItemsAsync(int id);
        Task<Cart> CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task<CartItem?> GetCartItemByIdAsync(int id);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
        Task ClearCartAsync(int cartId);
    }
}
