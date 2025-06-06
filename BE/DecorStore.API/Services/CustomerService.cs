using DecorStore.API.DTOs;
using DecorStore.API.Common;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DecorStore.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<CustomerDTO>>> GetPagedCustomersAsync(CustomerFilterDTO filter)
        {
            if (filter == null)
            {
                return Result<PagedResult<CustomerDTO>>.Failure("Filter cannot be null", "INVALID_INPUT");
            }

            try
            {
                var pagedCustomers = await _unitOfWork.Customers.GetPagedAsync(filter);
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(pagedCustomers.Items);

                var result = new PagedResult<CustomerDTO>(customerDtos, pagedCustomers.Pagination.TotalCount,
                    pagedCustomers.Pagination.CurrentPage, pagedCustomers.Pagination.PageSize);

                return Result<PagedResult<CustomerDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PagedResult<CustomerDTO>>.Failure($"Failed to retrieve customers: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<CustomerDTO>>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
                return Result<IEnumerable<CustomerDTO>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure($"Failed to retrieve all customers: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<CustomerDTO>> GetCustomerByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Result<CustomerDTO>.Failure("Customer ID must be greater than 0", "INVALID_INPUT");
            }

            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return Result<CustomerDTO>.NotFound("Customer");
                }

                var customerDto = _mapper.Map<CustomerDTO>(customer);
                return Result<CustomerDTO>.Success(customerDto);
            }
            catch (Exception ex)
            {
                return Result<CustomerDTO>.Failure($"Failed to retrieve customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<CustomerDTO>> GetCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<CustomerDTO>.Failure("Email cannot be empty", "INVALID_INPUT");
            }

            if (!IsValidEmail(email))
            {
                return Result<CustomerDTO>.Failure("Invalid email format", "INVALID_EMAIL");
            }

            try
            {
                var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
                if (customer == null)
                {
                    return Result<CustomerDTO>.NotFound("Customer");
                }

                var customerDto = _mapper.Map<CustomerDTO>(customer);
                return Result<CustomerDTO>.Success(customerDto);
            }
            catch (Exception ex)
            {
                return Result<CustomerDTO>.Failure($"Failed to retrieve customer by email: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<CustomerDTO>> CreateCustomerAsync(CreateCustomerDTO customerDto)
        {
            if (customerDto == null)
            {
                return Result<CustomerDTO>.Failure("Customer data cannot be null", "INVALID_INPUT");
            }

            // Validate input
            var validationResult = ValidateCreateCustomerDto(customerDto);
            if (validationResult.IsFailure)
            {
                return Result<CustomerDTO>.Failure(validationResult.Error!, validationResult.ErrorCode!, validationResult.ErrorDetails!);
            }

            try
            {
                // Check if email already exists
                if (await _unitOfWork.Customers.EmailExistsAsync(customerDto.Email))
                {
                    return Result<CustomerDTO>.Failure($"Customer with email {customerDto.Email} already exists", "DUPLICATE_EMAIL");
                }

                // Map DTO to entity
                var customer = _mapper.Map<Customer>(customerDto);

                await _unitOfWork.Customers.CreateAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var customerDto_result = _mapper.Map<CustomerDTO>(customer);
                return Result<CustomerDTO>.Success(customerDto_result);
            }
            catch (Exception ex)
            {
                return Result<CustomerDTO>.Failure($"Failed to create customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<CustomerDTO>> UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto)
        {
            if (id <= 0)
            {
                return Result<CustomerDTO>.Failure("Customer ID must be greater than 0", "INVALID_INPUT");
            }

            if (customerDto == null)
            {
                return Result<CustomerDTO>.Failure("Customer data cannot be null", "INVALID_INPUT");
            }

            // Validate input
            var validationResult = ValidateUpdateCustomerDto(customerDto);
            if (validationResult.IsFailure)
            {
                return Result<CustomerDTO>.Failure(validationResult.Error!, validationResult.ErrorCode!, validationResult.ErrorDetails!);
            }

            try
            {
                // Get customer or return not found
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                {
                    return Result<CustomerDTO>.NotFound("Customer");
                }

                // Map DTO to entity
                _mapper.Map(customerDto, customer);

                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var customerDto_result = _mapper.Map<CustomerDTO>(customer);
                return Result<CustomerDTO>.Success(customerDto_result);
            }
            catch (Exception ex)
            {
                return Result<CustomerDTO>.Failure($"Failed to update customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result> DeleteCustomerAsync(int id)
        {
            if (id <= 0)
            {
                return Result.Failure("Customer ID must be greater than 0", "INVALID_INPUT");
            }

            try
            {
                // Check if customer exists
                if (!await _unitOfWork.Customers.ExistsAsync(id))
                {
                    return Result.NotFound("Customer");
                }

                await _unitOfWork.Customers.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to delete customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // Advanced query methods
        public async Task<Result<IEnumerable<CustomerDTO>>> GetCustomersWithOrdersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetCustomersWithOrdersAsync();
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
                return Result<IEnumerable<CustomerDTO>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure($"Failed to retrieve customers with orders: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<CustomerDTO>>> GetTopCustomersByOrderCountAsync(int count = 10)
        {
            if (count <= 0)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
            }

            if (count > 100)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure("Count cannot exceed 100", "INVALID_INPUT");
            }

            try
            {
                var customers = await _unitOfWork.Customers.GetTopCustomersByOrderCountAsync(count);
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
                return Result<IEnumerable<CustomerDTO>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure($"Failed to retrieve top customers by order count: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<CustomerDTO>>> GetTopCustomersBySpendingAsync(int count = 10)
        {
            if (count <= 0)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure("Count must be greater than 0", "INVALID_INPUT");
            }

            if (count > 100)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure("Count cannot exceed 100", "INVALID_INPUT");
            }

            try
            {
                var customers = await _unitOfWork.Customers.GetTopCustomersBySpendingAsync(count);
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
                return Result<IEnumerable<CustomerDTO>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure($"Failed to retrieve top customers by spending: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<int>> GetOrderCountByCustomerAsync(int customerId)
        {
            if (customerId <= 0)
            {
                return Result<int>.Failure("Customer ID must be greater than 0", "INVALID_INPUT");
            }

            try
            {
                var count = await _unitOfWork.Customers.GetOrderCountByCustomerAsync(customerId);
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Failed to retrieve order count for customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<decimal>> GetTotalSpentByCustomerAsync(int customerId)
        {
            if (customerId <= 0)
            {
                return Result<decimal>.Failure("Customer ID must be greater than 0", "INVALID_INPUT");
            }

            try
            {
                var totalSpent = await _unitOfWork.Customers.GetTotalSpentByCustomerAsync(customerId);
                return Result<decimal>.Success(totalSpent);
            }
            catch (Exception ex)
            {
                return Result<decimal>.Failure($"Failed to retrieve total spent by customer: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<Result<IEnumerable<CustomerDTO>>> GetCustomersByLocationAsync(string? city = null, string? state = null, string? country = null)
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetCustomersByLocationAsync(city, state, country);
                var customerDtos = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
                return Result<IEnumerable<CustomerDTO>>.Success(customerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<CustomerDTO>>.Failure($"Failed to retrieve customers by location: {ex.Message}", "DATABASE_ERROR");
            }
        }

        // Private validation methods
        private Result ValidateCreateCustomerDto(CreateCustomerDTO customerDto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(customerDto.Email))
            {
                errors.Add("Email is required");
            }
            else if (!IsValidEmail(customerDto.Email))
            {
                errors.Add("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(customerDto.FirstName))
            {
                errors.Add("First name is required");
            }

            if (string.IsNullOrWhiteSpace(customerDto.LastName))
            {
                errors.Add("Last name is required");
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Phone) && !IsValidPhone(customerDto.Phone))
            {
                errors.Add("Invalid phone number format");
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        private Result ValidateUpdateCustomerDto(UpdateCustomerDTO customerDto)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(customerDto.Email) && !IsValidEmail(customerDto.Email))
            {
                errors.Add("Invalid email format");
            }

            if (!string.IsNullOrWhiteSpace(customerDto.Phone) && !IsValidPhone(customerDto.Phone))
            {
                errors.Add("Invalid phone number format");
            }

            if (errors.Any())
            {
                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhone(string phone)
        {
            try
            {
                // Allow various phone formats: +1-555-123-4567, (555) 123-4567, 555.123.4567, 5551234567
                var phoneRegex = new Regex(@"^[\+]?[1-9]?[\-\.\s]?\(?[0-9]{3}\)?[\-\.\s]?[0-9]{3}[\-\.\s]?[0-9]{4}$");
                return phoneRegex.IsMatch(phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", ""));
            }
            catch
            {
                return false;
            }
        }
    }
}
