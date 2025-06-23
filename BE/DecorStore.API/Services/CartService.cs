using DecorStore.API.DTOs;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Models;
using DecorStore.API.Common;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheInvalidationService _cacheInvalidationService;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, ICacheInvalidationService cacheInvalidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<Result<CartDTO>> GetCartAsync(int? userId, string? sessionId)
        {
            try
            {
                var cartResult = await GetOrCreateCartAsync(userId, sessionId);
                if (cartResult.IsFailure)
                {
                    return Result<CartDTO>.Failure(cartResult.ErrorCode!, cartResult.Error!);
                }

                var cartDto = MapCartToDTO(cartResult.Data!);
                return Result<CartDTO>.Success(cartDto);
            }
            catch (Exception ex)
            {
                return Result<CartDTO>.Failure("Failed to retrieve cart", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CartDTO>> AddToCartAsync(int? userId, string? sessionId, AddToCartDTO addToCartDto)
        {
            try
            {
                // Validate input
                if (addToCartDto == null)
                {
                    return Result<CartDTO>.Failure("INVALID_INPUT", "Add to cart data cannot be null");
                }

                if (addToCartDto.ProductId <= 0)
                {
                    return Result<CartDTO>.Failure("INVALID_PRODUCT_ID", "Product ID must be greater than 0");
                }

                if (addToCartDto.Quantity <= 0)
                {
                    return Result<CartDTO>.Failure("INVALID_QUANTITY", "Quantity must be greater than 0");
                }

                // Verify product exists and has stock
                var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId);
                if (product == null)
                {
                    return Result<CartDTO>.Failure("PRODUCT_NOT_FOUND", $"Product with ID {addToCartDto.ProductId} not found");
                }

                if (product.StockQuantity < addToCartDto.Quantity)
                {
                    return Result<CartDTO>.Failure("INSUFFICIENT_STOCK", $"Not enough stock. Available: {product.StockQuantity}");
                }

                // Get or create cart
                var cartResult = await GetOrCreateCartAsync(userId, sessionId);
                if (cartResult.IsFailure)
                {
                    return Result<CartDTO>.Failure(cartResult.ErrorCode!, cartResult.Error!);
                }

                var cart = cartResult.Data!;

                // Check if product already in cart
                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, addToCartDto.ProductId);

                if (cartItem != null)
                {
                    // Update quantity if product already in cart
                    var newQuantity = cartItem.Quantity + addToCartDto.Quantity;
                    
                    // Check if new quantity exceeds stock
                    if (newQuantity > product.StockQuantity)
                    {
                        return Result<CartDTO>.Failure("INSUFFICIENT_STOCK", $"Not enough stock. Available: {product.StockQuantity}");
                    }
                    
                    cartItem.Quantity = newQuantity;
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
                        UnitPrice = product.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Carts.AddCartItemAsync(cartItem);
                }

                // Update cart total
                var updateResult = await UpdateCartTotalAsync(cart.Id);
                if (updateResult.IsFailure)
                {
                    return Result<CartDTO>.Failure(updateResult.ErrorCode!, updateResult.Error!);
                }

                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache after successful cart update
                await _cacheInvalidationService.InvalidateCartCacheAsync(userId, sessionId);

                // Return updated cart
                return await GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                return Result<CartDTO>.Failure("Failed to add item to cart", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CartDTO>> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDTO updateCartItemDto)
        {
            try
            {
                // Validate input
                if (updateCartItemDto == null)
                {
                    return Result<CartDTO>.Failure("INVALID_INPUT", "Update cart item data cannot be null");
                }

                if (cartItemId <= 0)
                {
                    return Result<CartDTO>.Failure("INVALID_CART_ITEM_ID", "Cart item ID must be greater than 0");
                }

                if (updateCartItemDto.Quantity <= 0)
                {
                    return Result<CartDTO>.Failure("INVALID_QUANTITY", "Quantity must be greater than 0");
                }

                // Get cart
                var cartResult = await GetCartByIdentifiersAsync(userId, sessionId);
                if (cartResult == null)
                {
                    return Result<CartDTO>.Failure("CART_NOT_FOUND", "Cart not found");
                }

                // Get cart item
                var cartItem = await _unitOfWork.Carts.GetCartItemByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    return Result<CartDTO>.Failure("CART_ITEM_NOT_FOUND", "Cart item not found");
                }

                // Verify cart item belongs to the cart
                if (cartItem.CartId != cartResult.Id)
                {
                    return Result<CartDTO>.Failure("UNAUTHORIZED_ACCESS", "You don't have permission to update this cart item");
                }

                // Verify product has enough stock
                var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    return Result<CartDTO>.Failure("PRODUCT_NOT_FOUND", "Product not found");
                }

                if (product.StockQuantity < updateCartItemDto.Quantity)
                {
                    return Result<CartDTO>.Failure("INSUFFICIENT_STOCK", $"Not enough stock. Available: {product.StockQuantity}");
                }

                // Update quantity
                cartItem.Quantity = updateCartItemDto.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Carts.UpdateCartItemAsync(cartItem);

                // Update cart total
                var updateResult = await UpdateCartTotalAsync(cartResult.Id);
                if (updateResult.IsFailure)
                {
                    return Result<CartDTO>.Failure(updateResult.ErrorCode!, updateResult.Error!);
                }

                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache after successful cart item update
                await _cacheInvalidationService.InvalidateCartCacheAsync(userId, sessionId);

                // Return updated cart
                return await GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                return Result<CartDTO>.Failure("Failed to update cart item", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CartDTO>> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId)
        {
            try
            {
                // Validate input
                if (cartItemId <= 0)
                {
                    return Result<CartDTO>.Failure("INVALID_CART_ITEM_ID", "Cart item ID must be greater than 0");
                }

                // Get cart
                var cart = await GetCartByIdentifiersAsync(userId, sessionId);
                if (cart == null)
                {
                    return Result<CartDTO>.Failure("CART_NOT_FOUND", "Cart not found");
                }

                // Get cart item
                var cartItem = await _unitOfWork.Carts.GetCartItemByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    return Result<CartDTO>.Failure("CART_ITEM_NOT_FOUND", "Cart item not found");
                }

                // Verify cart item belongs to the cart
                if (cartItem.CartId != cart.Id)
                {
                    return Result<CartDTO>.Failure("UNAUTHORIZED_ACCESS", "You don't have permission to remove this cart item");
                }

                // Remove cart item
                await _unitOfWork.Carts.RemoveCartItemAsync(cartItem);

                // Update cart total
                var updateResult = await UpdateCartTotalAsync(cart.Id);
                if (updateResult.IsFailure)
                {
                    return Result<CartDTO>.Failure(updateResult.ErrorCode!, updateResult.Error!);
                }

                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache after successful cart item removal
                await _cacheInvalidationService.InvalidateCartCacheAsync(userId, sessionId);

                // Return updated cart
                return await GetCartAsync(userId, sessionId);
            }
            catch (Exception ex)
            {
                return Result<CartDTO>.Failure("Failed to remove cart item", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result<CartDTO>> ClearCartAsync(int? userId, string? sessionId)
        {
            try
            {
                // Get cart
                var cart = await GetCartByIdentifiersAsync(userId, sessionId);
                if (cart == null)
                {
                    return Result<CartDTO>.Failure("CART_NOT_FOUND", "Cart not found");
                }

                // Clear cart items
                await _unitOfWork.Carts.ClearCartAsync(cart.Id);

                // Update cart total
                cart.TotalAmount = 0;
                cart.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Carts.UpdateAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                // Invalidate cache after successful cart clearing
                await _cacheInvalidationService.InvalidateCartCacheAsync(userId, sessionId);

                // Return empty cart
                var cartDto = MapCartToDTO(cart);
                return Result<CartDTO>.Success(cartDto);
            }
            catch (Exception ex)
            {
                return Result<CartDTO>.Failure("Failed to clear cart", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        public async Task<Result> MergeCartsAsync(int userId, string sessionId)
        {
            try
            {
                // Validate input
                if (userId <= 0)
                {
                    return Result.Failure("INVALID_USER_ID", "User ID must be greater than 0");
                }

                if (string.IsNullOrEmpty(sessionId))
                {
                    return Result.Failure("INVALID_SESSION_ID", "Session ID cannot be null or empty");
                }

                // Get user cart
                var userCart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
                
                // Get session cart
                var sessionCart = await _unitOfWork.Carts.GetBySessionIdAsync(sessionId);

                // If no session cart or it's empty, nothing to merge
                if (sessionCart == null || !sessionCart.Items.Any())
                {
                    return Result.Success();
                }

                // If no user cart, convert session cart to user cart
                if (userCart == null)
                {
                    sessionCart.UserId = userId;
                    sessionCart.SessionId = null;
                    await _unitOfWork.Carts.UpdateAsync(sessionCart);
                    await _unitOfWork.SaveChangesAsync();
                    return Result.Success();
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
                var updateResult = await UpdateCartTotalAsync(userCart.Id);
                if (updateResult.IsFailure)
                {
                    return Result.Failure(updateResult.ErrorCode!, updateResult.Error!);
                }

                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to merge carts", "DATABASE_ERROR", new[] { ex.Message });
            }
        }

        // Helper methods
        private async Task<Result<Cart>> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            try
            {
                // Validate that at least one identifier is provided
                if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
                {
                    return Result<Cart>.Failure("INVALID_IDENTIFIERS", "Either user ID or session ID must be provided");
                }

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

                return Result<Cart>.Success(cart);
            }
            catch (Exception ex)
            {
                return Result<Cart>.Failure("Failed to get or create cart", "DATABASE_ERROR", new[] { ex.Message });
            }
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

        private async Task<Result> UpdateCartTotalAsync(int cartId)
        {
            try
            {
                var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId);
                if (cart == null)
                {
                    return Result.Failure("CART_NOT_FOUND", "Cart not found");
                }

                cart.TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice);
                cart.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Carts.UpdateAsync(cart);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to update cart total", "DATABASE_ERROR", new[] { ex.Message });
            }
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
