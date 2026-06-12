using bancoSol.DTOs;
using bancoSol.Models;

namespace bancoSol.Mappers
{
    public static class AccountMapper
    {
        public static AccountResponse ToResponse(this Account account)
        {
            return new AccountResponse
            {
                AccountNumber = account.AccountNumber,
                CustomerName = account.Customer != null
    ? string.Join(" ", new[]
        {
            account.Customer.FirstName,
            account.Customer.SecondName,
            account.Customer.FirstLastName,
            account.Customer.SecondLastName
        }.Where(x => !string.IsNullOrWhiteSpace(x)))
    : "",
                Currency = account.Currency,
                Balance = account.Balance,
                Status = account.Status,
                CreatedAt = account.CreatedAt
            };
        }
    }
}
