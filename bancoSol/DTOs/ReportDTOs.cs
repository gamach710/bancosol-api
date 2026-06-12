// DTOs/ConsolidatedBalanceDto.cs
namespace bancoSol.DTOs
{
    public class ConsolidatedBalanceRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class ConsolidatedBalanceResponse
    {
        public string Currency { get; set; } = string.Empty;
        public decimal TotalBalance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public bool IsFallbackRate { get; set; }
        public int TransactionCount { get; set; }
    }
}