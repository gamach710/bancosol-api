using bancoSol.Models;

namespace bancoSol.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(long id);
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        Task<List<Account>> GetByCustomerIdAsync(long customerId);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task<Account> CreateAsync(Account account);
        Task<bool> AccountNumberExistsAsync(string accountNumber);
        Task<Account?> GetByCustomerAndCurrencyAsync(long customerId, string currency);
        Task<Account?> GetLastAccountAsync();
        Task<Account?> GetByAccountNumberForUpdateAsync(string accountNumber);
        Task UpdateWithoutSaveAsync(Account account);
    }
}