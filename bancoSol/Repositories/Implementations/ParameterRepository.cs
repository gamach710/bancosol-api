using bancoSol.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bancoSol.Data;
using bancoSol.Models;

namespace bancoSol.Repositories.Implementations
{
    public class ParameterRepository : IParameterRepository
    {
        private readonly ApplicationDbContext _context;

        public ParameterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Parameter>> GetAllAsync()
            => await _context.Parameters.ToListAsync();

        public async Task<Parameter?> GetByIdAsync(int id)
            => await _context.Parameters.FindAsync(id);

        public async Task<List<Parameter>> GetByCategoryAsync(string category)
            => await _context.Parameters
                .Where(x => x.Category == category && x.IsActive)
                .ToListAsync();

        public async Task<List<string>> GetCodesByCategoryAsync(string category)
            => await _context.Parameters
                .Where(p => p.Category == category && p.IsActive)
                .Select(p => p.Code)
                .ToListAsync();

        public async Task<Parameter?> GetByCodeAndCategoryAsync(string category, string code)
            => await _context.Parameters
                .FirstOrDefaultAsync(p => p.Category == category && p.Code == code && p.IsActive);
    }
}