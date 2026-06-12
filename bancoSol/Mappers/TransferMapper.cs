using bancoSol.DTOs;
using bancoSol.Models;

namespace bancoSol.Mappers
{
    public static class TransferMapper
    {
        public static TransferResponse ToResponse(this Transfer transfer)
        {
            return new TransferResponse
            {
                Id = transfer.Id,
                SourceAccountNumber = transfer.SourceAccount?.AccountNumber
                    ?? string.Empty,
                DestinationAccountNumber = transfer.DestinationAccount?.AccountNumber
                    ?? string.Empty,
                SourceAmount = transfer.SourceAmount,
                DestinationAmount = transfer.DestinationAmount,
                SourceCurrency = transfer.SourceCurrency,
                DestinationCurrency = transfer.DestinationCurrency,
                ExchangeRate = transfer.ExchangeRate,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt
            };
        }

        
        public static TransferResponse ToResponse(
            this Transfer transfer,
            string sourceAccountNumber,
            string destinationAccountNumber)
        {
            return new TransferResponse
            {
                Id = transfer.Id,
                SourceAccountNumber = sourceAccountNumber,
                DestinationAccountNumber = destinationAccountNumber,
                SourceAmount = transfer.SourceAmount,
                DestinationAmount = transfer.DestinationAmount,
                SourceCurrency = transfer.SourceCurrency,
                DestinationCurrency = transfer.DestinationCurrency,
                ExchangeRate = transfer.ExchangeRate,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt
            };
        }
    }
}