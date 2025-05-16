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
        
        // Transaction management
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        // Save changes
        Task<int> SaveChangesAsync();
    }
}
