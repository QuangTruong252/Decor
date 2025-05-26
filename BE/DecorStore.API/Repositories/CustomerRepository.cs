using DecorStore.API.Data;
using DecorStore.API.Models;
using DecorStore.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecorStore.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            customer.UpdatedAt = DateTime.UtcNow;
            _context.Customers.Update(customer);
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true;
                customer.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<PagedResult<Customer>> GetPagedAsync(CustomerFilterDTO filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var query = BuildCustomerQuery(filter);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var items = await query
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Customer>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<int> GetTotalCountAsync(CustomerFilterDTO filter)
        {
            var query = BuildCustomerQuery(filter);
            return await query.CountAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithOrdersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted && c.Orders.Any(o => !o.IsDeleted))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
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
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .CountAsync();
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

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(c => c.City != null && c.City.ToLower().Contains(city.ToLower()));
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(c => c.State != null && c.State.ToLower().Contains(state.ToLower()));
            }

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(c => c.Country != null && c.Country.ToLower().Contains(country.ToLower()));
            }

            return await query
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        private IQueryable<Customer> BuildCustomerQuery(CustomerFilterDTO filter)
        {
            var query = _context.Customers.AsQueryable();

            // Apply base filters
            if (!filter.IncludeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(searchTerm)) ||
                    (c.Address != null && c.Address.ToLower().Contains(searchTerm)));
            }

            // Apply email filter
            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(filter.Email.ToLower()));
            }

            // Apply location filters
            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(c => c.City != null && c.City.ToLower().Contains(filter.City.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.State))
            {
                query = query.Where(c => c.State != null && c.State.ToLower().Contains(filter.State.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Country))
            {
                query = query.Where(c => c.Country != null && c.Country.ToLower().Contains(filter.Country.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.PostalCode))
            {
                query = query.Where(c => c.PostalCode != null && c.PostalCode.ToLower().Contains(filter.PostalCode.ToLower()));
            }

            // Apply date range filters
            if (filter.RegisteredAfter.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filter.RegisteredAfter.Value);
            }

            if (filter.RegisteredBefore.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filter.RegisteredBefore.Value);
            }

            // Apply orders filter
            if (filter.HasOrders.HasValue)
            {
                if (filter.HasOrders.Value)
                {
                    query = query.Where(c => c.Orders.Any(o => !o.IsDeleted));
                }
                else
                {
                    query = query.Where(c => !c.Orders.Any(o => !o.IsDeleted));
                }
            }

            // Include related data based on filter options
            if (filter.IncludeOrderCount || filter.IncludeTotalSpent)
            {
                query = query.Include(c => c.Orders);
            }

            return query;
        }

        private IQueryable<Customer> ApplySorting(IQueryable<Customer> query, PaginationParameters filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
            {
                return query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
            }

            return filter.SortBy.ToLower() switch
            {
                "firstname" => filter.IsDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
                "lastname" => filter.IsDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
                "email" => filter.IsDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                "createdat" => filter.IsDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                "city" => filter.IsDescending ? query.OrderByDescending(c => c.City) : query.OrderBy(c => c.City),
                "state" => filter.IsDescending ? query.OrderByDescending(c => c.State) : query.OrderBy(c => c.State),
                "country" => filter.IsDescending ? query.OrderByDescending(c => c.Country) : query.OrderBy(c => c.Country),
                "ordercount" => filter.IsDescending ?
                    query.OrderByDescending(c => c.Orders.Count(o => !o.IsDeleted)) :
                    query.OrderBy(c => c.Orders.Count(o => !o.IsDeleted)),
                "totalspent" => filter.IsDescending ?
                    query.OrderByDescending(c => c.Orders.Where(o => !o.IsDeleted).Sum(o => o.TotalAmount)) :
                    query.OrderBy(c => c.Orders.Where(o => !o.IsDeleted).Sum(o => o.TotalAmount)),
                _ => query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            };
        }
    }
}
