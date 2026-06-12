using bancoSol.Data;
using bancoSol.DTOs;
using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using bancoSol.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace bancoSol.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> GetOrCreateAsync(CreateAccountRequest request)
        {
        
            var customer = await _customerRepository.GetByCIAsync(request.CI);

          
            if (customer == null)
            {
                customer = new Customer
                {
                    FirstName = request.FirstName,
                    SecondName = request.SecondName,
                    FirstLastName = request.FirstLastName,
                    SecondLastName = request.SecondLastName,
                    CI = request.CI,
                    Email = request.Email,
                    Phone = request.Phone,
                    CreatedAt = DateTime.UtcNow
                };
                await _customerRepository.AddAsync(customer);
            }

            return customer;
        }
    }
}