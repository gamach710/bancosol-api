using System.Security.Cryptography.Xml;

namespace bancoSol.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public long CustomerId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Customer Customer { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Transfer> TransfersOut { get; set; } = new List<Transfer>();
        public ICollection<Transfer> TransfersIn { get; set; } = new List<Transfer>();
    }
}