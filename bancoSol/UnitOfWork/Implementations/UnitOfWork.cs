using bancoSol.Data;
using bancoSol.Repositories.Implementations;
using bancoSol.Repositories.Interfaces;

namespace bancoSol.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IAccountRepository Accounts { get; }
        public ICustomerRepository Customers { get; }
        public ITransactionRepository Transactions { get; }
        public ITransferRepository Transfers { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IAccountRepository accounts,
            ICustomerRepository customers,
            ITransactionRepository transactions,
            ITransferRepository transfers)
        {
            _context = context;
            Accounts = accounts;
            Customers = customers;
            Transactions = transactions;
            Transfers = transfers;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}