using System.Security.Principal;

namespace bancoSol.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; }
        public string FirstLastName { get; set; } = string.Empty;
        public string? SecondLastName { get; set; }
        public string CI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}