using System;
using System.Threading.Tasks;
using DecorStore.API.Repositories;

namespace DecorStore.API.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IReviewRepository Reviews { get; }
        IBannerRepository Banners { get; }
        ICartRepository Carts { get; }
        ICustomerRepository Customers { get; }
        IDashboardRepository Dashboard { get; }

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
