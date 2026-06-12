using bancoSol.Constants;
using bancoSol.DTOs;
using bancoSol.Mappers;
using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using bancoSol.Services.Interfaces;
using bancoSol.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;

namespace bancoSol.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IParameterRepository _parameterRepository;
        private readonly ICustomerService _customerService;
        private readonly IMemoryCache _cache;

        public AccountService(
            IUnitOfWork unitOfWork,
            ICustomerService customerService,
            IParameterRepository parameterRepository,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _parameterRepository = parameterRepository;
            _customerService = customerService;
            _cache = cache;
        }

        public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
        {
            var validCurrencies = new[] { Currency.BOB, Currency.USD };
            if (!validCurrencies.Contains(request.Currency.ToUpper()))
                throw new ArgumentException($"Moneda '{request.Currency}' no soportada. Use BOB o USD.");

            var customer = await _customerService.GetOrCreateAsync(request);
            var accountNumber = await GenerateAccountNumberAsync();

            var account = new Account
            {
                AccountNumber = accountNumber,
                CustomerId = customer.Id,
                Currency = request.Currency.ToUpper(),
                Balance = request.InitialBalance,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Accounts.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            if (request.InitialBalance > 0)
            {
                await _unitOfWork.Transactions.AddAsync(new Transaction
                {
                    AccountId = account.Id,
                    Type = TransactionType.Deposit,
                    Amount = request.InitialBalance,
                    PreviousBalance = 0,
                    NewBalance = request.InitialBalance,
                    Description = $"Depósito inicial en {request.Currency.ToUpper()}",
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
            }

            return account.ToResponse();
        }

        public async Task<AccountResponse> GetByIdAsync(long id)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(id);
            if (account == null)
                throw new KeyNotFoundException($"Cuenta con ID '{id}' no encontrada.");

            return account.ToResponse();
        }

        public async Task<AccountResponse> GetAccountByNumberAsync(string accountNumber)
        {
            var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                throw new KeyNotFoundException($"Cuenta '{accountNumber}' no encontrada.");

            return account.ToResponse();
        }

        public async Task<List<AccountResponse>> GetByCustomerIdAsync(long customerId)
        {
            var accounts = await _unitOfWork.Accounts.GetByCustomerIdAsync(customerId);
            return accounts.Select(a => a.ToResponse()).ToList();
        }

        public async Task<AccountResponse> DepositAsync(string accountNumber, DepositWithdrawRequest request)
        {
            var account = await _unitOfWork.Accounts.GetByAccountNumberForUpdateAsync(accountNumber.Trim());

            if (account == null)
                throw new KeyNotFoundException($"Cuenta '{accountNumber}' no encontrada.");

            if (account.Status != AccountStatus.Active)
                throw new InvalidOperationException("La cuenta no está activa o está bloqueada.");

            var previousBalance = account.Balance;
            account.Balance = Math.Round(account.Balance + request.Amount, 2);

            await _unitOfWork.Accounts.UpdateWithoutSaveAsync(account);

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = Math.Round(request.Amount, 2),
                PreviousBalance = previousBalance,
                NewBalance = account.Balance,
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? $"Depósito en cuenta {accountNumber}"
                    : request.Description,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"transactions_{accountNumber}");
            return account.ToResponse();
        }

        public async Task<AccountResponse> WithdrawAsync(string accountNumber, DepositWithdrawRequest request)
        {
            var account = await _unitOfWork.Accounts.GetByAccountNumberForUpdateAsync(accountNumber.Trim());

            if (account == null)
                throw new KeyNotFoundException($"Cuenta '{accountNumber}' no encontrada.");

            if (account.Status != AccountStatus.Active)
                throw new InvalidOperationException("La cuenta no está activa o está bloqueada.");

            if (account.Balance < request.Amount)
                throw new ArgumentException(
                    $"Saldo insuficiente. Saldo disponible: {account.Balance} {account.Currency}.");

            var previousBalance = account.Balance;
            account.Balance = Math.Round(account.Balance - request.Amount, 2);

            if (account.Balance < 0)
                throw new ArgumentException("El saldo no puede quedar negativo.");

            await _unitOfWork.Accounts.UpdateWithoutSaveAsync(account);

            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Withdrawal,
                Amount = Math.Round(request.Amount, 2),
                PreviousBalance = previousBalance,
                NewBalance = account.Balance,
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? $"Retiro de cuenta {accountNumber}"
                    : request.Description,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"transactions_{accountNumber}");
            return account.ToResponse();
        }

        public async Task<PagedTransactionsResponse> GetTransactionsAsync(string accountNumber, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var cacheKey = $"transactions_{accountNumber}";

            if (_cache.TryGetValue(cacheKey, out PagedTransactionsResponse? cached) && cached != null)
                return cached;

            var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(accountNumber.Trim());
            if (account == null)
                throw new KeyNotFoundException($"Cuenta '{accountNumber}' no encontrada.");

            var transactions = await _unitOfWork.Transactions
                .GetByAccountIdPagedAsync(account.Id, page, pageSize);
            var total = await _unitOfWork.Transactions.CountByAccountIdAsync(account.Id);

            var result = new PagedTransactionsResponse
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Data = transactions.Select(t => t.ToResponse()).ToList()
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        private async Task<string> GenerateAccountNumberAsync()
        {
            var lastAccount = await _unitOfWork.Accounts.GetLastAccountAsync();

            if (lastAccount == null)
            {
                var initialParam = await _parameterRepository.GetByCodeAndCategoryAsync("ACCOUNT", "INITIAL_NUMBER");
                if (initialParam == null)
                    throw new InvalidOperationException("No se encontró el parámetro de número inicial de cuenta.");

                return initialParam.Description;
            }

            if (!long.TryParse(lastAccount.AccountNumber, out long lastNumber))
                throw new InvalidOperationException("No se pudo generar el número de cuenta. Formato inválido.");

            return (lastNumber + 1).ToString();
        }
    }
}