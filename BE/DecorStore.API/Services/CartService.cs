using DecorStore.API.DTOs;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDTO> GetCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            return MapCartToDTO(cart);
        }

        public async Task<CartDTO> AddToCartAsync(int? userId, string? sessionId, AddToCartDTO addToCartDto)
        {
            // Verify product exists and has stock
            var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId)
                ?? throw new NotFoundException("Product not found");

            if (product.StockQuantity < addToCartDto.Quantity)
            {
                throw new InvalidOperationException($"Not enough stock. Available: {product.StockQuantity}");
            }

            // Get or create cart
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            // Check if product already in cart
            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, addToCartDto.ProductId);

            if (cartItem != null)
            {
                // Update quantity if product already in cart
                cartItem.Quantity += addToCartDto.Quantity;
                
                // Check if new quantity exceeds stock
                if (cartItem.Quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Not enough stock. Available: {product.StockQuantity}");
                }
                
                cartItem.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Carts.UpdateCartItemAsync(cartItem);
            }
            else
            {
                // Add new cart item
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = product.Price
                };
                await _unitOfWork.Carts.AddCartItemAsync(cartItem);
            }

            // Update cart total
            await UpdateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();

            // Return updated cart
            return await GetCartAsync(userId, sessionId);
        }

        public async Task<CartDTO> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDTO updateCartItemDto)
        {
            // Get cart
            var cart = await GetCartByIdentifiersAsync(userId, sessionId)
                ?? throw new NotFoundException("Cart not found");

            // Get cart item
            var cartItem = await _unitOfWork.Carts.GetCartItemByIdAsync(cartItemId)
                ?? throw new NotFoundException("Cart item not found");

            // Verify cart item belongs to the cart
            if (cartItem.CartId != cart.Id)
            {
                throw new UnauthorizedException("You don't have permission to update this cart item");
            }

            // Verify product has enough stock
            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId)
                ?? throw new NotFoundException("Product not found");

            if (product.StockQuantity < updateCartItemDto.Quantity)
            {
                throw new InvalidOperationException($"Not enough stock. Available: {product.StockQuantity}");
            }

            // Update quantity
            cartItem.Quantity = updateCartItemDto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Carts.UpdateCartItemAsync(cartItem);

            // Update cart total
            await UpdateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();

            // Return updated cart
            return await GetCartAsync(userId, sessionId);
        }

        public async Task<CartDTO> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId)
        {
            // Get cart
            var cart = await GetCartByIdentifiersAsync(userId, sessionId)
                ?? throw new NotFoundException("Cart not found");

            // Get cart item
            var cartItem = await _unitOfWork.Carts.GetCartItemByIdAsync(cartItemId)
                ?? throw new NotFoundException("Cart item not found");

            // Verify cart item belongs to the cart
            if (cartItem.CartId != cart.Id)
            {
                throw new UnauthorizedException("You don't have permission to remove this cart item");
            }

            // Remove cart item
            await _unitOfWork.Carts.RemoveCartItemAsync(cartItem);

            // Update cart total
            await UpdateCartTotalAsync(cart.Id);
            await _unitOfWork.SaveChangesAsync();

            // Return updated cart
            return await GetCartAsync(userId, sessionId);
        }

        public async Task<CartDTO> ClearCartAsync(int? userId, string? sessionId)
        {
            // Get cart
            var cart = await GetCartByIdentifiersAsync(userId, sessionId)
                ?? throw new NotFoundException("Cart not found");

            // Clear cart items
            await _unitOfWork.Carts.ClearCartAsync(cart.Id);

            // Update cart total
            cart.TotalAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Carts.UpdateAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            // Return empty cart
            return MapCartToDTO(cart);
        }

        public async Task MergeCartsAsync(int userId, string sessionId)
        {
            // Get user cart
            var userCart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            
            // Get session cart
            var sessionCart = await _unitOfWork.Carts.GetBySessionIdAsync(sessionId);

            // If no session cart or it's empty, nothing to merge
            if (sessionCart == null || !sessionCart.Items.Any())
            {
                return;
            }

            // If no user cart, convert session cart to user cart
            if (userCart == null)
            {
                sessionCart.UserId = userId;
                sessionCart.SessionId = null;
                await _unitOfWork.Carts.UpdateAsync(sessionCart);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            // Merge items from session cart to user cart
            foreach (var item in sessionCart.Items)
            {
                var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);

                if (existingItem != null)
                {
                    // Update quantity if product already in user cart
                    existingItem.Quantity += item.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Carts.UpdateCartItemAsync(existingItem);
                }
                else
                {
                    // Add new cart item to user cart
                    var newItem = new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Carts.AddCartItemAsync(newItem);
                }
            }

            // Clear session cart
            await _unitOfWork.Carts.ClearCartAsync(sessionCart.Id);

            // Update user cart total
            await UpdateCartTotalAsync(userCart.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        // Helper methods
        private async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetCartByIdentifiersAsync(userId, sessionId);

            if (cart == null)
            {
                // Create new cart
                cart = new Cart
                {
                    UserId = userId,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Carts.CreateAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<Cart?> GetCartByIdentifiersAsync(int? userId, string? sessionId)
        {
            if (userId.HasValue)
            {
                return await _unitOfWork.Carts.GetByUserIdAsync(userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await _unitOfWork.Carts.GetBySessionIdAsync(sessionId);
            }

            return null;
        }

        private async Task UpdateCartTotalAsync(int cartId)
        {
            var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId)
                ?? throw new NotFoundException("Cart not found");

            cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice);
            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Carts.UpdateAsync(cart);
        }

        private CartDTO MapCartToDTO(Cart cart)
        {
            var cartDto = new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                TotalAmount = cart.TotalAmount,
                TotalItems = cart.Items.Sum(i => i.Quantity),
                UpdatedAt = cart.UpdatedAt,
                Items = new List<CartItemDTO>()
            };

            foreach (var item in cart.Items)
            {
                var productImage = item.Product.Images.FirstOrDefault()?.FilePath;
                
                cartDto.Items.Add(new CartItemDTO
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSlug = item.Product.Slug,
                    ProductImage = productImage,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Quantity * item.UnitPrice
                });
            }

            return cartDto;
        }
    }
}
