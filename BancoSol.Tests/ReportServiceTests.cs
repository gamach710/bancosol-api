using bancoSol.Constants;
using bancoSol.Data;
using bancoSol.DTOs;
using bancoSol.Models;
using bancoSol.Repositories.Implementations;
using bancoSol.Services.Implementations;
using bancoSol.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BancoSol.Tests
{
    public class ReportServiceTests
    {
        private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
        private readonly Mock<ILogger<ReportService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly ReportService _reportService;

        private const decimal ExchangeRate = 6.94m;

       
        private static readonly DateTime Start = new DateTime(2026, 5, 1);
        private static readonly DateTime End = new DateTime(2026, 6, 30);

        public ReportServiceTests()
        {
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _loggerMock = new Mock<ILogger<ReportService>>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);

            var transactionRepository = new TransactionRepository(_context);

            _reportService = new ReportService(
                transactionRepository,
                _exchangeRateServiceMock.Object,
                _loggerMock.Object);

            SeedData();
        }

        /// <summary>
        /// Datos de prueba sembrados en la base de datos en memoria.
        ///
        /// Cuenta BOB (Id=1):
        ///   + Depósito   1000 BOB
        ///   + TransferIn  200 BOB
        ///   - TransferOut 100 BOB
        ///
        /// Cuenta USD (Id=2):
        ///   + Depósito    100 USD
        ///   - Retiro       20 USD
        /// </summary>
        private void SeedData()
        {
            var bobAccount = new Account
            {
                Id = 1,
                AccountNumber = "10000056365010",
                CustomerId = 1,
                Currency = Currency.BOB,
                Balance = 1100,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var usdAccount = new Account
            {
                Id = 2,
                AccountNumber = "10000056365011",
                CustomerId = 1,
                Currency = Currency.USD,
                Balance = 80,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.AddRange(bobAccount, usdAccount);
            _context.SaveChanges();

            var referenceDate = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            
            _context.Transactions.Add(new Transaction
            {
                AccountId = bobAccount.Id,
                Type = TransactionType.Deposit,
                Amount = 1000,
                PreviousBalance = 0,
                NewBalance = 1000,
                Description = "Depósito inicial BOB",
                CreatedAt = referenceDate
            });

            
            _context.Transactions.Add(new Transaction
            {
                AccountId = bobAccount.Id,
                Type = TransactionType.TransferIn,
                Amount = 200,
                PreviousBalance = 1000,
                NewBalance = 1200,
                Description = "Transferencia entrante BOB",
                CreatedAt = referenceDate
            });

           
            _context.Transactions.Add(new Transaction
            {
                AccountId = bobAccount.Id,
                Type = TransactionType.TransferOut,
                Amount = 100,
                PreviousBalance = 1200,
                NewBalance = 1100,
                Description = "Transferencia saliente BOB",
                CreatedAt = referenceDate
            });

           
            _context.Transactions.Add(new Transaction
            {
                AccountId = usdAccount.Id,
                Type = TransactionType.Deposit,
                Amount = 100,
                PreviousBalance = 0,
                NewBalance = 100,
                Description = "Depósito inicial USD",
                CreatedAt = referenceDate
            });

            
            _context.Transactions.Add(new Transaction
            {
                AccountId = usdAccount.Id,
                Type = TransactionType.Withdrawal,
                Amount = 20,
                PreviousBalance = 100,
                NewBalance = 80,
                Description = "Retiro USD",
                CreatedAt = referenceDate
            });

            _context.SaveChanges();
        }

        
        private void SetupExchangeRate(bool isFallback = false)
        {
            _exchangeRateServiceMock
                .Setup(s => s.GetRateAsync())
                .ReturnsAsync(new ExchangeRateResponse
                {
                    From = "USD",
                    To = "BOB",
                    Rate = ExchangeRate,
                    IsFallback = isFallback,
                    FetchedAt = DateTime.UtcNow
                });
        }

        // ─────────────────────────────────────────────
        // CONVERSIÓN Y TOTALES
        // ─────────────────────────────────────────────

        // PRUEBA 1: Reporte en BOB convierte USD correctamente y suma/resta todos los tipos
        //
        // BOB puro:  +1000 +200 -100 = +1100
        // USD→BOB:   +100*6.94 -20*6.94 = +694 -138.80 = +555.20
        // Total BOB: 1100 + 555.20 = 1655.20
        [Fact]
        public async Task ConsolidatedBalance_InBOB_ConvertsAndSumsAllTransactionTypes()
        {
            SetupExchangeRate();

            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = Start,
                EndDate = End,
                Currency = "BOB"
            });

            Assert.Equal("BOB", result.Currency);
            Assert.Equal(1655.20m, result.TotalBalance);
            Assert.Equal(ExchangeRate, result.ExchangeRateUsed);
            Assert.Equal(5, result.TransactionCount);
        }

        // PRUEBA 2: Reporte en USD convierte BOB correctamente y suma/resta todos los tipos
      
        [Fact]
        public async Task ConsolidatedBalance_InUSD_ConvertsAndSumsAllTransactionTypes()
        {
            SetupExchangeRate();

            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = Start,
                EndDate = End,
                Currency = "USD"
            });

            Assert.Equal("USD", result.Currency);
            Assert.Equal(238.50m, result.TotalBalance);
            Assert.Equal(ExchangeRate, result.ExchangeRateUsed);
            Assert.Equal(5, result.TransactionCount);
        }

        // PRUEBA 3: La moneda en la petición se normaliza a mayúsculas
        [Fact]
        public async Task ConsolidatedBalance_LowercaseCurrency_NormalizesToUppercase()
        {
            SetupExchangeRate();

            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = Start,
                EndDate = End,
                Currency = "bob"   
            });

            Assert.Equal("BOB", result.Currency);
        }

        // ─────────────────────────────────────────────
        // FILTRO POR FECHAS
        // ─────────────────────────────────────────────

        // PRUEBA 4: Fechas fuera del rango de transacciones devuelven balance 0
        [Fact]
        public async Task ConsolidatedBalance_OutsideDateRange_ReturnsZeroBalance()
        {
            SetupExchangeRate();

            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = new DateTime(2027, 1, 1),
                EndDate = new DateTime(2027, 1, 31),
                Currency = "BOB"
            });

            Assert.Equal(0, result.TransactionCount);
            Assert.Equal(0m, result.TotalBalance);
        }

        // PRUEBA 5: El EndDate es inclusivo (transacciones del último día se incluyen)
        [Fact]
        public async Task ConsolidatedBalance_EndDateIsInclusive()
        {
            SetupExchangeRate();

       
            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 6, 1),
                Currency = "BOB"
            });

            Assert.Equal(5, result.TransactionCount);
        }

        // ─────────────────────────────────────────────
        // TASA DE CAMBIO
        // ─────────────────────────────────────────────

        // PRUEBA 6: Si la tasa es de fallback, el reporte lo indica correctamente
        [Fact]
        public async Task ConsolidatedBalance_FallbackRate_ReflectedInResponse()
        {
            SetupExchangeRate(isFallback: true);

            var result = await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = Start,
                EndDate = End,
                Currency = "BOB"
            });

            Assert.True(result.IsFallbackRate);
        }

        // PRUEBA 7: El tipo de cambio se consulta exactamente una vez por reporte
        [Fact]
        public async Task ConsolidatedBalance_ExchangeRateService_CalledExactlyOnce()
        {
            SetupExchangeRate();

            await _reportService.GetConsolidatedBalanceAsync(new ConsolidatedBalanceRequest
            {
                StartDate = Start,
                EndDate = End,
                Currency = "BOB"
            });

            _exchangeRateServiceMock.Verify(s => s.GetRateAsync(), Times.Once);
        }
    }
}
