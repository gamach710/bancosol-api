using bancoSol.Constants;
using bancoSol.DTOs;
using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using bancoSol.Services.Implementations;
using bancoSol.Services.Interfaces;
using bancoSol.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BancoSol.Tests
{
    public class TransferServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<ITransferRepository> _transferRepoMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IExchangeRateService> _exchangeServiceMock;
        private readonly Mock<ILogger<TransferService>> _loggerMock;
        private readonly TransferService _service;

        public TransferServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _transferRepoMock = new Mock<ITransferRepository>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _exchangeServiceMock = new Mock<IExchangeRateService>();
            _loggerMock = new Mock<ILogger<TransferService>>();

            _unitOfWorkMock.Setup(u => u.Accounts).Returns(_accountRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Transfers).Returns(_transferRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Transactions).Returns(_transactionRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateWithoutSaveAsync(It.IsAny<Account>()))
                .Returns(Task.CompletedTask);

            _transferRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Transfer>()))
                .Returns(Task.CompletedTask);

            _transactionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

        
            _transferRepoMock
                .Setup(x => x.GetByIdempotencyKeyAsync(It.IsAny<string>()))
                .ReturnsAsync((Transfer?)null);

            _service = new TransferService(
                _unitOfWorkMock.Object,
                _exchangeServiceMock.Object,
                _loggerMock.Object);
        }

        // ─────────────────────────────────────────────
        // CONVERSIÓN DE MONEDA
        // ─────────────────────────────────────────────

        // PRUEBA 1: Transferencia USD → BOB multiplica por la tasa
        [Fact]
        public async Task Transfer_USDtoBOB_MultipliesByRate()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 500, Currency = Currency.USD, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("200"))
                .ReturnsAsync(new Account { Id = 2, AccountNumber = "200", Balance = 1000, Currency = Currency.BOB, Status = AccountStatus.Active });

            _exchangeServiceMock
                .Setup(x => x.GetRateAsync())
                .ReturnsAsync(new ExchangeRateResponse { Rate = 7 });

            var result = await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "100",
                DestinationAccountNumber = "200",
                Amount = 100,
                IdempotencyKey = "key1"
            });

           
            Assert.Equal(700, result.DestinationAmount);
            Assert.Equal(7, result.ExchangeRate);
        }

        // PRUEBA 2: Transferencia BOB → USD divide por la tasa
        [Fact]
        public async Task Transfer_BOBtoUSD_DividesByRate()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("300"))
                .ReturnsAsync(new Account { Id = 3, AccountNumber = "300", Balance = 700, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("400"))
                .ReturnsAsync(new Account { Id = 4, AccountNumber = "400", Balance = 100, Currency = Currency.USD, Status = AccountStatus.Active });

            _exchangeServiceMock
                .Setup(x => x.GetRateAsync())
                .ReturnsAsync(new ExchangeRateResponse { Rate = 7 });

            var result = await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "300",
                DestinationAccountNumber = "400",
                Amount = 700,
                IdempotencyKey = "key2"
            });

         
            Assert.Equal(100, result.DestinationAmount);
            Assert.Equal(7, result.ExchangeRate);
        }

        // PRUEBA 3: Misma moneda — no llama al servicio de tipo de cambio
        [Fact]
        public async Task Transfer_SameCurrency_NoExchangeRateCalled()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("500"))
                .ReturnsAsync(new Account { Id = 5, AccountNumber = "500", Balance = 1000, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("600"))
                .ReturnsAsync(new Account { Id = 6, AccountNumber = "600", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active });

            var result = await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "500",
                DestinationAccountNumber = "600",
                Amount = 300,
                IdempotencyKey = "key3"
            });

            Assert.Equal(300, result.DestinationAmount);
            Assert.Null(result.ExchangeRate);

           
            _exchangeServiceMock.Verify(x => x.GetRateAsync(), Times.Never);
        }

        // ─────────────────────────────────────────────
        // SALDOS
        // ─────────────────────────────────────────────

        // PRUEBA 4: El saldo de origen se descuenta correctamente
        [Fact]
        public async Task Transfer_SourceBalance_IsReducedByAmount()
        {
            var source = new Account { Id = 1, AccountNumber = "100", Balance = 1000, Currency = Currency.BOB, Status = AccountStatus.Active };
            var destination = new Account { Id = 2, AccountNumber = "200", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active };

            _accountRepoMock.Setup(x => x.GetByAccountNumberAsync("100")).ReturnsAsync(source);
            _accountRepoMock.Setup(x => x.GetByAccountNumberAsync("200")).ReturnsAsync(destination);

            await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "100",
                DestinationAccountNumber = "200",
                Amount = 300,
                IdempotencyKey = "key4"
            });

       
            Assert.Equal(700, source.Balance);
        }

        // PRUEBA 5: El saldo de destino se incrementa correctamente
        [Fact]
        public async Task Transfer_DestinationBalance_IsIncreasedByAmount()
        {
            var source = new Account { Id = 1, AccountNumber = "100", Balance = 1000, Currency = Currency.BOB, Status = AccountStatus.Active };
            var destination = new Account { Id = 2, AccountNumber = "200", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active };

            _accountRepoMock.Setup(x => x.GetByAccountNumberAsync("100")).ReturnsAsync(source);
            _accountRepoMock.Setup(x => x.GetByAccountNumberAsync("200")).ReturnsAsync(destination);

            await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "100",
                DestinationAccountNumber = "200",
                Amount = 300,
                IdempotencyKey = "key5"
            });

           
            Assert.Equal(800, destination.Balance);
        }

        // PRUEBA 6: Saldo insuficiente lanza ArgumentException
        [Fact]
        public async Task Transfer_InsufficientBalance_ThrowsArgumentException()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 50, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("200"))
                .ReturnsAsync(new Account { Id = 2, AccountNumber = "200", Balance = 100, Currency = Currency.BOB, Status = AccountStatus.Active });

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateTransferAsync(new CreateTransferRequest
                {
                    SourceAccountNumber = "100",
                    DestinationAccountNumber = "200",
                    Amount = 200,
                    IdempotencyKey = "key6"
                }));

            Assert.Contains("Saldo insuficiente", exception.Message);
        }

        // ─────────────────────────────────────────────
        // IDEMPOTENCIA
        // ─────────────────────────────────────────────

        // PRUEBA 7: Reintento con misma clave devuelve resultado anterior sin ejecutar de nuevo
        [Fact]
        public async Task Transfer_SameIdempotencyKey_ReturnsSameResultWithoutSaving()
        {
            var existingTransfer = new Transfer
            {
                Id = 1,
                SourceAccountId = 1,
                DestinationAccountId = 2,
                SourceAmount = 100,
                DestinationAmount = 700,
                SourceCurrency = Currency.USD,
                DestinationCurrency = Currency.BOB,
                ExchangeRate = 7,
                IdempotencyKey = "key-repetida",
                Status = TransferStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                SourceAccount = new Account { AccountNumber = "100" },
                DestinationAccount = new Account { AccountNumber = "200" }
            };

            _transferRepoMock
                .Setup(x => x.GetByIdempotencyKeyAsync("key-repetida"))
                .ReturnsAsync(existingTransfer);

            var result = await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "100",
                DestinationAccountNumber = "200",
                Amount = 100,
                IdempotencyKey = "key-repetida"
            });

            Assert.Equal(700, result.DestinationAmount);

       
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
            _transferRepoMock.Verify(u => u.AddAsync(It.IsAny<Transfer>()), Times.Never);
        }

        // ─────────────────────────────────────────────
        // CUENTAS NO ENCONTRADAS
        // ─────────────────────────────────────────────

        // PRUEBA 8: Cuenta origen no encontrada lanza KeyNotFoundException
        [Fact]
        public async Task Transfer_SourceAccountNotFound_ThrowsKeyNotFoundException()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateTransferAsync(new CreateTransferRequest
                {
                    SourceAccountNumber = "999",
                    DestinationAccountNumber = "200",
                    Amount = 100,
                    IdempotencyKey = "key8"
                }));
        }

        // PRUEBA 9: Cuenta destino no encontrada lanza KeyNotFoundException
        [Fact]
        public async Task Transfer_DestinationAccountNotFound_ThrowsKeyNotFoundException()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("999"))
                .ReturnsAsync((Account?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateTransferAsync(new CreateTransferRequest
                {
                    SourceAccountNumber = "100",
                    DestinationAccountNumber = "999",
                    Amount = 100,
                    IdempotencyKey = "key9"
                }));
        }

        // ─────────────────────────────────────────────
        // ESTADOS DE CUENTA
        // ─────────────────────────────────────────────

        // PRUEBA 10: Cuenta origen inactiva lanza InvalidOperationException
        [Fact]
        public async Task Transfer_InactiveSourceAccount_ThrowsInvalidOperationException()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Inactive });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("200"))
                .ReturnsAsync(new Account { Id = 2, AccountNumber = "200", Balance = 100, Currency = Currency.BOB, Status = AccountStatus.Active });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateTransferAsync(new CreateTransferRequest
                {
                    SourceAccountNumber = "100",
                    DestinationAccountNumber = "200",
                    Amount = 100,
                    IdempotencyKey = "key10"
                }));
        }

        // PRUEBA 11: Cuenta destino bloqueada lanza InvalidOperationException
        [Fact]
        public async Task Transfer_BlockedDestinationAccount_ThrowsInvalidOperationException()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("200"))
                .ReturnsAsync(new Account { Id = 2, AccountNumber = "200", Balance = 100, Currency = Currency.BOB, Status = AccountStatus.Blocked });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateTransferAsync(new CreateTransferRequest
                {
                    SourceAccountNumber = "100",
                    DestinationAccountNumber = "200",
                    Amount = 100,
                    IdempotencyKey = "key11"
                }));
        }

        // ─────────────────────────────────────────────
        // PERSISTENCIA
        // ─────────────────────────────────────────────

        // PRUEBA 12: Una transferencia exitosa guarda exactamente 1 Transfer y 2 Transactions
        [Fact]
        public async Task Transfer_Success_SavesTransferAndTwoTransactions()
        {
            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("100"))
                .ReturnsAsync(new Account { Id = 1, AccountNumber = "100", Balance = 1000, Currency = Currency.BOB, Status = AccountStatus.Active });

            _accountRepoMock
                .Setup(x => x.GetByAccountNumberAsync("200"))
                .ReturnsAsync(new Account { Id = 2, AccountNumber = "200", Balance = 500, Currency = Currency.BOB, Status = AccountStatus.Active });

            await _service.CreateTransferAsync(new CreateTransferRequest
            {
                SourceAccountNumber = "100",
                DestinationAccountNumber = "200",
                Amount = 100,
                IdempotencyKey = "key12"
            });

            _transferRepoMock.Verify(r => r.AddAsync(It.IsAny<Transfer>()), Times.Once);
            _transactionRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
