using bancoSol.DTOs;
using bancoSol.Models;

namespace bancoSol.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> GetOrCreateAsync(CreateAccountRequest request);
    }
}
