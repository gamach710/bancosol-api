using bancoSol.Models;

namespace bancoSol.Repositories.Interfaces
{
    public interface ITransferRepository
    {
        Task<Transfer?> GetByIdAsync(long id);
        Task<Transfer?> GetByIdempotencyKeyAsync(string key);
        Task AddAsync(Transfer transfer);
    }
}