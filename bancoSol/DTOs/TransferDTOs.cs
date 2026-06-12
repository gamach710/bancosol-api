namespace bancoSol.DTOs
{
    public class CreateTransferRequest
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public class TransferResponse
    {
        public long Id { get; set; }
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal SourceAmount { get; set; }
        public decimal DestinationAmount { get; set; }
        public string SourceCurrency { get; set; } = string.Empty;
        public string DestinationCurrency { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}