using DecorStore.API.DTOs;
using DecorStore.API.Models;
using DecorStore.API.Exceptions;
using DecorStore.API.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

        public async Task<CustomerDTO> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Customer with ID {id} not found");

            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO> GetCustomerByEmailAsync(string email)
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email)
                ?? throw new NotFoundException($"Customer with email {email} not found");

            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<Customer> CreateCustomerAsync(CreateCustomerDTO customerDto)
        {
            // Check if email already exists
            if (await _unitOfWork.Customers.EmailExistsAsync(customerDto.Email))
            {
                throw new InvalidOperationException($"Customer with email {customerDto.Email} already exists");
            }

            // Map DTO to entity
            var customer = _mapper.Map<Customer>(customerDto);

            await _unitOfWork.Customers.CreateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateCustomerAsync(int id, UpdateCustomerDTO customerDto)
        {
            // Get customer or throw if not found
            var customer = await _unitOfWork.Customers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Customer with ID {id} not found");

            // Map DTO to entity
            _mapper.Map(customerDto, customer);

            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            // Check if customer exists
            if (!await _unitOfWork.Customers.ExistsAsync(id))
            {
                throw new NotFoundException($"Customer with ID {id} not found");
            }

            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
