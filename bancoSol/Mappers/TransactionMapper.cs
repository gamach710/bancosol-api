// Mappers/TransactionMapper.cs
using bancoSol.DTOs;
using bancoSol.Models;

namespace bancoSol.Mappers
{
    public static class TransactionMapper
    {
        public static TransactionResponse ToResponse(this Transaction t)
        {
            return new TransactionResponse
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                PreviousBalance = t.PreviousBalance,
                NewBalance = t.NewBalance,
                Description = t.Description,
                ExchangeRate = t.ExchangeRate,
                CreatedAt = t.CreatedAt
            };
        }
    }
}