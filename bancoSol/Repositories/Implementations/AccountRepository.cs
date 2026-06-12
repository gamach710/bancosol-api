using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bancoSol.Data;

namespace bancoSol.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(long id)
            => await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
            => await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);

        public async Task<List<Account>> GetByCustomerIdAsync(long customerId)
            => await _context.Accounts
                .Include(a => a.Customer)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

        public async Task<Account?> GetByCustomerAndCurrencyAsync(long customerId, string currency)
            => await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.CustomerId == customerId &&
                    a.Currency == currency.ToUpper());

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            return Task.CompletedTask;
        }

        public async Task<Account> CreateAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            return account;
        }

        public async Task<Account?> GetByAccountNumberForUpdateAsync(string accountNumber)
        {
            return await _context.Accounts
                .FromSqlInterpolated($@"
            SELECT *
            FROM accounts
            WHERE account_number = {accountNumber}
            FOR UPDATE")
                .FirstOrDefaultAsync();
        }

        public Task UpdateWithoutSaveAsync(Account account)
        {
            _context.Accounts.Update(account);
            return Task.CompletedTask;
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber)
            => await _context.Accounts
                .AnyAsync(a => a.AccountNumber == accountNumber);

        public async Task<Account?> GetLastAccountAsync()
            => await _context.Accounts
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
    }
}