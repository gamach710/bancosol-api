using bancoSol.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bancoSol.Models;
using bancoSol.Data;

namespace bancoSol.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetByAccountIdAsync(long accountId)
            => await _context.Transactions
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

        public async Task<List<Transaction>> GetByAccountIdPagedAsync(long accountId, int page, int pageSize)
            => await _context.Transactions
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> CountByAccountIdAsync(long accountId)
            => await _context.Transactions
                .Where(x => x.AccountId == accountId)
                .CountAsync();

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
        }

        public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
            => await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToListAsync();
    }
}