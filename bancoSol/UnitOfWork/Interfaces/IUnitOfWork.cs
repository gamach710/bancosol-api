using bancoSol.Repositories.Interfaces;

namespace bancoSol.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }
        ICustomerRepository Customers { get; }
        ITransactionRepository Transactions { get; }
        ITransferRepository Transfers { get; }
        Task SaveChangesAsync();
    }
}