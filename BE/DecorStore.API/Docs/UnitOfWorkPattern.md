# Unit of Work Pattern Implementation

## Overview

The Unit of Work pattern is a design pattern that maintains a list of objects affected by a business transaction and coordinates the writing out of changes and the resolution of concurrency problems. In our e-commerce application, this pattern helps manage database transactions and ensures data consistency.

## Key Components

### 1. IUnitOfWork Interface

The `IUnitOfWork` interface defines the contract for the Unit of Work pattern:

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    IReviewRepository Reviews { get; }
    IBannerRepository Banners { get; }
    
    // Transaction management
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    
    // Save changes
    Task<int> SaveChangesAsync();
}
```

### 2. UnitOfWork Implementation

The `UnitOfWork` class implements the `IUnitOfWork` interface:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    
    // Repository properties
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_context);
    // Other repositories...
    
    // Transaction management methods
    public async Task BeginTransactionAsync() { ... }
    public async Task CommitTransactionAsync() { ... }
    public async Task RollbackTransactionAsync() { ... }
    
    // Save changes method
    public async Task<int> SaveChangesAsync() { ... }
}
```

### 3. Repository Pattern

Each repository focuses on data access operations for a specific entity:

```csharp
public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    
    // Data access methods without SaveChanges calls
    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        return category;
    }
    
    // Other methods...
}
```

## Benefits

1. **Centralized Transaction Management**: The Unit of Work pattern centralizes transaction management, making it easier to maintain data consistency.

2. **Simplified Repository Implementation**: Repositories focus solely on data access operations, without worrying about transaction management.

3. **Improved Testability**: The pattern makes it easier to mock repositories and unit test services.

4. **Reduced Duplicate Code**: Eliminates duplicate transaction management code across services.

## Usage Example

Here's an example of using the Unit of Work pattern with transactions:

```csharp
public async Task<Order> CreateOrderWithTransactionAsync(CreateOrderDTO orderDto, int userId)
{
    // Begin transaction
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        // Create order
        var order = new Order { ... };
        await _unitOfWork.Orders.CreateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Create order items
        foreach (var item in orderDto.Items)
        {
            // Get product and check stock
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            
            // Create order item
            var orderItem = new OrderItem { ... };
            _unitOfWork.Orders.AddOrderItem(orderItem);

            // Update product stock
            product.StockQuantity -= item.Quantity;
            await _unitOfWork.Products.UpdateAsync(product);
        }

        // Save all changes
        await _unitOfWork.SaveChangesAsync();

        // Commit transaction
        await _unitOfWork.CommitTransactionAsync();

        return order;
    }
    catch (Exception)
    {
        // Rollback transaction on error
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## Implementation Steps

1. Create the `IUnitOfWork` interface
2. Implement the `UnitOfWork` class
3. Update repository interfaces and implementations to work with the Unit of Work pattern
4. Register the Unit of Work in the dependency injection container
5. Update services to use the Unit of Work pattern
