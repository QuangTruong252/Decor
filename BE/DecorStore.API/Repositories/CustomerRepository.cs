using DecorStore.API.Models;
using DecorStore.API.Data;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DecorStore.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Customer>> GetPagedAsync(CustomerFilterDTO filter)
        {
            var query = GetFilteredCustomers(filter);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Customer>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers
                .AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email && !c.IsDeleted);
        }

        public async Task<int> GetTotalCountAsync(CustomerFilterDTO filter)
        {
            return await GetFilteredCustomers(filter).CountAsync();
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true;
                await UpdateAsync(customer);
            }
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithOrdersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted && c.Orders.Any(o => !o.IsDeleted))
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetTopCustomersByOrderCountAsync(int count = 10)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Orders.Count(o => !o.IsDeleted))
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetTopCustomersBySpendingAsync(int count = 10)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Orders.Where(o => !o.IsDeleted).Sum(o => o.TotalAmount))
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetOrderCountByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .CountAsync(o => o.CustomerId == customerId && !o.IsDeleted);
        }

        public async Task<decimal> GetTotalSpentByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByLocationAsync(string? city = null, string? state = null, string? country = null)
        {
            var query = _context.Customers.Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(c => c.City == city);

            if (!string.IsNullOrWhiteSpace(state))
                query = query.Where(c => c.State == state);

            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(c => c.Country == country);

            return await query.ToListAsync();
        }

        private IQueryable<Customer> GetFilteredCustomers(CustomerFilterDTO filter)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.Phone.Contains(searchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(c => c.Email == filter.Email);

            if (!string.IsNullOrWhiteSpace(filter.City))
                query = query.Where(c => c.City == filter.City);

            if (!string.IsNullOrWhiteSpace(filter.State))
                query = query.Where(c => c.State == filter.State);

            if (!string.IsNullOrWhiteSpace(filter.Country))
                query = query.Where(c => c.Country == filter.Country);

            if (!string.IsNullOrWhiteSpace(filter.PostalCode))
                query = query.Where(c => c.PostalCode == filter.PostalCode);

            if (filter.RegisteredAfter.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.RegisteredAfter);

            if (filter.RegisteredBefore.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.RegisteredBefore);

            if (filter.HasOrders.HasValue)
            {
                if (filter.HasOrders.Value)
                    query = query.Where(c => c.Orders.Any());
                else
                    query = query.Where(c => !c.Orders.Any());
            }

            if (!filter.IncludeDeleted)
                query = query.Where(c => !c.IsDeleted);

            return query;
        }
    }
}
