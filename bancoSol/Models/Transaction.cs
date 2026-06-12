namespace bancoSol.Models
{
    public class Transaction
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal PreviousBalance { get; set; }

        public decimal NewBalance { get; set; }

        public string? Description { get; set; }

        public decimal? ExchangeRate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Account Account { get; set; } = null!;
    }
}