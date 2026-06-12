using bancoSol.Models;

namespace bancoSol.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(long id);
        Task<Customer> GetByCiAsync(string ci);
        Task<Customer> GetByEmailAsync(string email);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task<Customer?> GetByCIAsync(string ci);
    }
}
