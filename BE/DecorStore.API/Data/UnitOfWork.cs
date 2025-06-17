using System;
using System.Threading.Tasks;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Repositories;
using DecorStore.API.Interfaces.Services;
using DecorStore.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DecorStore.API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private IDbContextTransaction? _transaction;        private IProductRepository? _productRepository;
        private ICategoryRepository? _categoryRepository;
        private IImageRepository? _imageRepository;
        private IOrderRepository? _orderRepository;
        private IReviewRepository? _reviewRepository;
        private IBannerRepository? _bannerRepository;
        private ICartRepository? _cartRepository;
        private ICustomerRepository? _customerRepository;
        private IDashboardRepository? _dashboardRepository;        private IUserRepository? _userRepository;
        private IOrderItemRepository? _orderItemRepository;
        private IRefreshTokenRepository? _refreshTokenRepository;
        private ISecurityEventRepository? _securityEventRepository;

        public UnitOfWork(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public ApplicationDbContext Context => _context;

        public IProductRepository Products => _productRepository ??= new ProductRepository(_context, _cacheService);
        public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_context, _cacheService);
        public IImageRepository Images => _imageRepository ??= new ImageRepository(_context);
        public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
        public IReviewRepository Reviews => _reviewRepository ??= new ReviewRepository(_context);
        public IBannerRepository Banners => _bannerRepository ??= new BannerRepository(_context);
        public ICartRepository Carts => _cartRepository ??= new CartRepository(_context);
        public ICustomerRepository Customers => _customerRepository ??= new CustomerRepository(_context);
        public IDashboardRepository Dashboard => _dashboardRepository ??= new DashboardRepository(_context);        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IOrderItemRepository OrderItems => _orderItemRepository ??= new OrderItemRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
        public ISecurityEventRepository SecurityEvents => _securityEventRepository ??= new SecurityEventRepository(_context);        public async Task BeginTransactionAsync()
        {
            // Skip transactions for in-memory database
            if (_context.Database.IsInMemory())
            {
                return;
            }
            
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            // Skip transactions for in-memory database
            if (_context.Database.IsInMemory())
            {
                return;
            }
            
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
            // Skip transactions for in-memory database
            if (_context.Database.IsInMemory())
            {
                return;
            }
            
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
        }        public async Task<TResult> ExecuteWithExecutionStrategyAsync<TResult>(Func<Task<TResult>> operation)
        {
            // Skip execution strategy for in-memory database
            if (_context.Database.IsInMemory())
            {
                return await operation();
            }
            
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(operation);
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
