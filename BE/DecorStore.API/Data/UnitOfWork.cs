using System;
using System.Threading.Tasks;
using DecorStore.API.Interfaces;
using DecorStore.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DecorStore.API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IProductRepository? _productRepository;
        private ICategoryRepository? _categoryRepository;
        private IOrderRepository? _orderRepository;
        private IReviewRepository? _reviewRepository;
        private IBannerRepository? _bannerRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
        public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_context);
        public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
        public IReviewRepository Reviews => _reviewRepository ??= new ReviewRepository(_context);
        public IBannerRepository Banners => _bannerRepository ??= new BannerRepository(_context);

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
        }
    }
}
