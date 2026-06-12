using bancoSol.DTOs;
using bancoSol.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bancoSol.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var result = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { accountNumber = result.AccountNumber }, result);
        }

        [HttpGet("{accountNumber}")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAccount(string accountNumber)
        {
            var result = await _accountService.GetAccountByNumberAsync(accountNumber);
            return Ok(result);
        }

        [HttpPost("{accountNumber}/deposits")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Deposit(string accountNumber, [FromBody] DepositWithdrawRequest request)
        {
            var result = await _accountService.DepositAsync(accountNumber, request);
            return Ok(result);
        }

        [HttpPost("{accountNumber}/withdrawals")]
        [ProducesResponseType(typeof(AccountResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Withdraw(string accountNumber, [FromBody] DepositWithdrawRequest request)
        {
            var result = await _accountService.WithdrawAsync(accountNumber, request);
            return Ok(result);
        }

        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(typeof(PagedTransactionsResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTransactions(
            string accountNumber,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetTransactionsAsync(accountNumber, page, pageSize);
            return Ok(result);
        }
    }
}