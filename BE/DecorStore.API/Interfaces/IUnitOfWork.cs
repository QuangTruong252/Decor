using System;
using System.Threading.Tasks;
using DecorStore.API.Interfaces.Repositories;

namespace DecorStore.API.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // DbContext access
        Data.ApplicationDbContext Context { get; }
        
        // Repositories
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IImageRepository Images { get; }
        IOrderRepository Orders { get; }
        IReviewRepository Reviews { get; }
        IBannerRepository Banners { get; }
        ICartRepository Carts { get; }
        ICustomerRepository Customers { get; }
        IDashboardRepository Dashboard { get; }        IUserRepository Users { get; }
        IOrderItemRepository OrderItems { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        ISecurityEventRepository SecurityEvents { get; }

        // Transaction management
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Save changes
        Task<int> SaveChangesAsync();

        // Execute with execution strategy
        Task<TResult> ExecuteWithExecutionStrategyAsync<TResult>(Func<Task<TResult>> operation);
    }
}
