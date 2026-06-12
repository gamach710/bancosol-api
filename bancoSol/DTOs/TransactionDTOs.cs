namespace bancoSol.DTOs
{
    public class TransactionResponse
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public string? Description { get; set; }
        public decimal? ExchangeRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PagedTransactionsResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<TransactionResponse> Data { get; set; } = new();
    }
}