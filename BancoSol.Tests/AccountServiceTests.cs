using bancoSol.Constants;
using bancoSol.DTOs;
using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using bancoSol.Services.Implementations;
using bancoSol.Services.Interfaces;
using bancoSol.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace BancoSol.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IParameterRepository> _parameterRepoMock;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly IMemoryCache _cache;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _accountRepoMock = new Mock<IAccountRepository>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _parameterRepoMock = new Mock<IParameterRepository>();
            _customerServiceMock = new Mock<ICustomerService>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            _unitOfWorkMock.Setup(u => u.Accounts).Returns(_accountRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Transactions).Returns(_transactionRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            _accountRepoMock
                .Setup(r => r.UpdateWithoutSaveAsync(It.IsAny<Account>()))
                .Returns(Task.CompletedTask);

            _transactionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            _accountService = new AccountService(
                _unitOfWorkMock.Object,
                _customerServiceMock.Object,
                _parameterRepoMock.Object,
                _cache);
        }

        // PRUEBA 1: Moneda no soportada
        [Fact]
        public async Task CreateAccount_UnsupportedCurrency_ThrowsArgumentException()
        {
            var request = new CreateAccountRequest
            {
                FirstName = "Juan",
                FirstLastName = "Perez",
                CI = "12345678",
                Email = "juan@gmail.com",
                Currency = "EUR",
                InitialBalance = 100
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _accountService.CreateAccountAsync(request));

            Assert.Contains("EUR", exception.Message);
        }

        // PRUEBA 2: Retiro con saldo insuficiente
        [Fact]
        public async Task Withdraw_InsufficientBalance_ThrowsArgumentException()
        {
            var account = new Account
            {
                Id = 1,
                AccountNumber = "10000056365001",
                Balance = 100,
                Currency = "BOB",
                Status = AccountStatus.Active
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365001"))
                .ReturnsAsync(account);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _accountService.WithdrawAsync("10000056365001",
                    new DepositWithdrawRequest { Amount = 500, Description = "Retiro" }));

            Assert.Contains("Saldo insuficiente", exception.Message);
        }

        // PRUEBA 3: Depósito exitoso
        [Fact]
        public async Task Deposit_ValidAmount_UpdatesBalance()
        {
            var account = new Account
            {
                Id = 1,
                AccountNumber = "10000056365001",
                Balance = 500,
                Currency = "BOB",
                Status = AccountStatus.Active
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365001"))
                .ReturnsAsync(account);

            var result = await _accountService.DepositAsync("10000056365001",
                new DepositWithdrawRequest { Amount = 200, Description = "Depósito" });

            Assert.Equal(700, result.Balance);
        }

        // PRUEBA 4: Cuenta inactiva no puede depositar
        [Fact]
        public async Task Deposit_InactiveAccount_ThrowsInvalidOperationException()
        {
            var account = new Account
            {
                Id = 2,
                AccountNumber = "10000056365006",
                Balance = 200,
                Currency = "USD",
                Status = AccountStatus.Inactive
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365006"))
                .ReturnsAsync(account);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _accountService.DepositAsync("10000056365006",
                    new DepositWithdrawRequest { Amount = 100, Description = "Depósito inactiva" }));
        }

        // PRUEBA 5: Retiro exitoso
        [Fact]
        public async Task Withdraw_ValidAmount_UpdatesBalance()
        {
            var account = new Account
            {
                Id = 3,
                AccountNumber = "10000056365002",
                Balance = 500,
                Currency = "BOB",
                Status = AccountStatus.Active
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365002"))
                .ReturnsAsync(account);

            var result = await _accountService.WithdrawAsync("10000056365002",
                new DepositWithdrawRequest { Amount = 200, Description = "Retiro" });

            Assert.Equal(300, result.Balance);
        }

        // PRUEBA 6: Cuenta bloqueada no puede operar
        [Fact]
        public async Task Withdraw_BlockedAccount_ThrowsInvalidOperationException()
        {
            var account = new Account
            {
                Id = 4,
                AccountNumber = "10000056365003",
                Balance = 500,
                Currency = "BOB",
                Status = AccountStatus.Blocked
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365003"))
                .ReturnsAsync(account);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _accountService.WithdrawAsync("10000056365003",
                    new DepositWithdrawRequest { Amount = 100, Description = "Retiro bloqueada" }));

            Assert.Contains("no está activa", exception.Message);
        }

        // PRUEBA 7: Cuenta no encontrada
        [Fact]
        public async Task GetAccount_NotFound_ThrowsKeyNotFoundException()
        {
            _accountRepoMock
                .Setup(r => r.GetByAccountNumberAsync("9999999999"))
                .ReturnsAsync((Account?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _accountService.GetAccountByNumberAsync("9999999999"));

            Assert.Contains("9999999999", exception.Message);
        }

        // PRUEBA 8: Depósito en cuenta bloqueada
        [Fact]
        public async Task Deposit_BlockedAccount_ThrowsInvalidOperationException()
        {
            var account = new Account
            {
                Id = 5,
                AccountNumber = "10000056365004",
                Balance = 300,
                Currency = "BOB",
                Status = AccountStatus.Blocked
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365004"))
                .ReturnsAsync(account);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _accountService.DepositAsync("10000056365004",
                    new DepositWithdrawRequest { Amount = 100, Description = "Depósito bloqueada" }));
        }

        // PRUEBA 9: Retiro que deja saldo en exactamente 0
        [Fact]
        public async Task Withdraw_ExactBalance_LeavesZero()
        {
            var account = new Account
            {
                Id = 6,
                AccountNumber = "10000056365005",
                Balance = 200,
                Currency = "USD",
                Status = AccountStatus.Active
            };

            _accountRepoMock
                .Setup(r => r.GetByAccountNumberForUpdateAsync("10000056365005"))
                .ReturnsAsync(account);

            var result = await _accountService.WithdrawAsync("10000056365005",
                new DepositWithdrawRequest { Amount = 200, Description = "Retiro total" });

            Assert.Equal(0, result.Balance);
        }

        // PRUEBA 10: Moneda BOB es válida
        [Fact]
        public async Task CreateAccount_BOB_Currency_IsValid()
        {
            var customer = new bancoSol.Models.Customer
            {
                Id = 1,
                FirstName = "Ana",
                FirstLastName = "Lopez",
                CI = "87654321",
                Email = "ana@gmail.com"
            };

            _customerServiceMock
                .Setup(s => s.GetOrCreateAsync(It.IsAny<CreateAccountRequest>()))
                .ReturnsAsync(customer);

            _accountRepoMock
                .Setup(r => r.GetLastAccountAsync())
                .ReturnsAsync((Account?)null);

            _parameterRepoMock
                .Setup(p => p.GetByCodeAndCategoryAsync("ACCOUNT", "INITIAL_NUMBER"))
                .ReturnsAsync(new bancoSol.Models.Parameter { Description = "10000000000001" });

            _accountRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            var request = new CreateAccountRequest
            {
                FirstName = "Ana",
                FirstLastName = "Lopez",
                CI = "87654321",
                Email = "ana@gmail.com",
                Currency = "BOB",
                InitialBalance = 0
            };

            var result = await _accountService.CreateAccountAsync(request);

            Assert.Equal("BOB", result.Currency);
        }
    }
}