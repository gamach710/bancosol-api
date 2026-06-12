using bancoSol.Models;

namespace bancoSol.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetByAccountIdAsync(long accountId);
        Task<List<Transaction>> GetByAccountIdPagedAsync(long accountId, int page, int pageSize);
        Task<int> CountByAccountIdAsync(long accountId);
        Task AddAsync(Transaction transaction);
        Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}