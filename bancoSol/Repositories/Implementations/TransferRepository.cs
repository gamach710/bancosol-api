using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bancoSol.Data;

namespace bancoSol.Repositories.Implementations
{
    public class TransferRepository : ITransferRepository
    {
        private readonly ApplicationDbContext _context;

        public TransferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transfer?> GetByIdAsync(long id)
            => await _context.Transfers
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Transfer?> GetByIdempotencyKeyAsync(string key)
            => await _context.Transfers
                .Include(t => t.SourceAccount)
                .Include(t => t.DestinationAccount)
                .FirstOrDefaultAsync(t => t.IdempotencyKey == key);

        public async Task AddAsync(Transfer transfer)
        {
            await _context.Transfers.AddAsync(transfer);
        }
    }
}