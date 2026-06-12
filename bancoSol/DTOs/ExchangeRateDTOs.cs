namespace bancoSol.DTOs
{
    public class ExchangeRateResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public bool IsFallback { get; set; } // ✅ Indica si es tasa de respaldo
        public DateTime FetchedAt { get; set; }
    }
}