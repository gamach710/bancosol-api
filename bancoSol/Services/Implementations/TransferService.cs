using bancoSol.Constants;
using bancoSol.DTOs;
using bancoSol.Mappers;
using bancoSol.Models;
using bancoSol.Services.Interfaces;
using bancoSol.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace bancoSol.Services.Implementations
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<TransferService> _logger;

        public TransferService(
            IUnitOfWork unitOfWork,
            IExchangeRateService exchangeRateService,
            ILogger<TransferService> logger)
        {
            _unitOfWork = unitOfWork;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        public async Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request)
        {
            var existing = await _unitOfWork.Transfers.GetByIdempotencyKeyAsync(request.IdempotencyKey);

            if (existing != null)
                return existing.ToResponse();

            var firstNumber =
                string.Compare(request.SourceAccountNumber, request.DestinationAccountNumber) < 0
                ? request.SourceAccountNumber
                : request.DestinationAccountNumber;

            var secondNumber =
                firstNumber == request.SourceAccountNumber
                ? request.DestinationAccountNumber
                : request.SourceAccountNumber;

            var first = await _unitOfWork.Accounts.GetByAccountNumberAsync(firstNumber);
            var second = await _unitOfWork.Accounts.GetByAccountNumberAsync(secondNumber);

            var source = first?.AccountNumber == request.SourceAccountNumber ? first : second;
            var destination = first?.AccountNumber == request.DestinationAccountNumber ? first : second;

            if (source == null)
                throw new KeyNotFoundException("Cuenta origen no encontrada.");

            if (destination == null)
                throw new KeyNotFoundException("Cuenta destino no encontrada.");

            if (source.Status != AccountStatus.Active)
                throw new InvalidOperationException("Cuenta origen inactiva.");

            if (destination.Status != AccountStatus.Active)
                throw new InvalidOperationException("Cuenta destino inactiva.");

            if (source.Balance < request.Amount)
                throw new ArgumentException("Saldo insuficiente.");

            decimal destinationAmount = request.Amount;
            decimal? exchangeRate = null;

            if (source.Currency != destination.Currency)
            {
                var rate = await _exchangeRateService.GetRateAsync();
                exchangeRate = rate.Rate;

                destinationAmount =
                    source.Currency == Currency.USD && destination.Currency == Currency.BOB
                    ? request.Amount * rate.Rate
                    : request.Amount / rate.Rate;
            }

            var prevSource = source.Balance;
            var prevDest = destination.Balance;

            source.Balance -= request.Amount;
            destination.Balance += destinationAmount;

            await _unitOfWork.Accounts.UpdateWithoutSaveAsync(source);
            await _unitOfWork.Accounts.UpdateWithoutSaveAsync(destination);

            var transfer = new Transfer
            {
                SourceAccountId = source.Id,
                DestinationAccountId = destination.Id,
                SourceAmount = request.Amount,
                DestinationAmount = destinationAmount,
                SourceCurrency = source.Currency,
                DestinationCurrency = destination.Currency,
                ExchangeRate = exchangeRate,
                IdempotencyKey = request.IdempotencyKey,
                Status = TransferStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Transfers.AddAsync(transfer);

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = source.Id,
                Type = TransactionType.TransferOut,
                Amount = request.Amount,
                PreviousBalance = prevSource,
                NewBalance = source.Balance,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = destination.Id,
                Type = TransactionType.TransferIn,
                Amount = destinationAmount,
                PreviousBalance = prevDest,
                NewBalance = destination.Balance,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Transferencia realizada");

            return transfer.ToResponse(source.AccountNumber, destination.AccountNumber);
        }
    }
}