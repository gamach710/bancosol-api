using bancoSol.Models;
using bancoSol.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bancoSol.Data;


namespace bancoSol.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
            => await _context.Customers.ToListAsync();

        public async Task<Customer> GetByIdAsync(long id)
            => await _context.Customers.FindAsync(id);

        public async Task<Customer> GetByCiAsync(string ci)
            => await _context.Customers.FirstOrDefaultAsync(x => x.CI == ci);

        public async Task<Customer> GetByEmailAsync(string email)
            => await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer?> GetByCIAsync(string ci)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CI == ci);
        }
    }
}
