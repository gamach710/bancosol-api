using bancoSol.DTOs;

namespace bancoSol.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
        Task<AccountResponse> GetByIdAsync(long id);
        Task<AccountResponse> GetAccountByNumberAsync(string accountNumber);
        Task<List<AccountResponse>> GetByCustomerIdAsync(long customerId);
        Task<AccountResponse> DepositAsync(string accountNumber, DepositWithdrawRequest request);
        Task<AccountResponse> WithdrawAsync(string accountNumber, DepositWithdrawRequest request);
        Task<PagedTransactionsResponse> GetTransactionsAsync(string accountNumber, int page, int pageSize);
    }
}