namespace bancoSol.DTOs
{
    public class CreateAccountRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; }
        public string FirstLastName { get; set; } = string.Empty;
        public string? SecondLastName { get; set; }
        public string CI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
    }

    public class AccountResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class DepositWithdrawRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}