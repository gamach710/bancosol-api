using bancoSol.Constants;
using bancoSol.DTOs;
using bancoSol.Repositories.Interfaces;
using bancoSol.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace bancoSol.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ITransactionRepository transactionRepository,
            IExchangeRateService exchangeRateService,
            ILogger<ReportService> logger)
        {
            _transactionRepository = transactionRepository;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        public async Task<ConsolidatedBalanceResponse> GetConsolidatedBalanceAsync(
            ConsolidatedBalanceRequest request)
        {
            request.Currency = request.Currency.ToUpper();

            var rate = await _exchangeRateService.GetRateAsync();

            var endDate = request.EndDate.Date.AddDays(1).AddTicks(-1);

            
            var transactions = await _transactionRepository
                .GetByDateRangeAsync(request.StartDate, endDate);

            decimal total = 0;

            foreach (var t in transactions)
            {
                var amount = t.Amount;

                if (t.Account.Currency != request.Currency)
                {
                    amount = t.Account.Currency == "USD" && request.Currency == "BOB"
                        ? Math.Round(t.Amount * rate.Rate, 2)
                        : Math.Round(t.Amount / rate.Rate, 2);
                }

                if (t.Type == TransactionType.Deposit || t.Type == TransactionType.TransferIn)
                    total += amount;
                else if (t.Type == TransactionType.Withdrawal || t.Type == TransactionType.TransferOut)
                    total -= amount;
            }

            _logger.LogInformation(
                "Reporte consolidado | Currency: {Currency} | StartDate: {Start} | EndDate: {End} | Total: {Total}",
                request.Currency, request.StartDate, request.EndDate, Math.Round(total, 2));

            return new ConsolidatedBalanceResponse
            {
                Currency = request.Currency,
                TotalBalance = Math.Round(total, 2),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ExchangeRateUsed = rate.Rate,
                IsFallbackRate = rate.IsFallback,
                TransactionCount = transactions.Count
            };
        }
    }
}
