using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DecorStore.API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await _context.Carts
                .AsNoTracking()
                .AsSplitQuery() // Optimize complex includes
                .Include(c => c.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
        }

        public async Task<Cart?> GetBySessionIdAsync(string sessionId)
        {
            return await _context.Carts
                .AsNoTracking()
                .AsSplitQuery() // Optimize complex includes
                .Include(c => c.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && !c.IsDeleted);
        }

        public async Task<Cart?> GetByIdAsync(int id)
        {
            return await _context.Carts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Cart?> GetByIdWithItemsAsync(int id)
        {
            return await _context.Carts
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.ProductImages)
                            .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Cart> CreateAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            return cart;
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Entry(cart).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int id)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductImages)
                        .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductImages)
                        .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.Entry(cartItem).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await Task.CompletedTask;
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Where(i => i.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
        }
    }
}
