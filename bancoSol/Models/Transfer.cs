namespace bancoSol.Models
{
        public class Transfer
        {
            public long Id { get; set; }

            public long SourceAccountId { get; set; }

            public long DestinationAccountId { get; set; }

            public decimal SourceAmount { get; set; }

            public decimal DestinationAmount { get; set; }

            public string SourceCurrency { get; set; } = string.Empty;

            public string DestinationCurrency { get; set; } = string.Empty;

            public decimal? ExchangeRate { get; set; }

            public string IdempotencyKey { get; set; } = string.Empty;

            public string Status { get; set; } = "Completed";

            public DateTime CreatedAt { get; set; }

            public Account SourceAccount { get; set; } = null!;

            public Account DestinationAccount { get; set; } = null!;
        }
    }

