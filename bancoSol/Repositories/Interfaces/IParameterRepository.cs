using bancoSol.Models;

namespace bancoSol.Repositories.Interfaces
{
    public interface IParameterRepository
    {
        Task<List<Parameter>> GetAllAsync();
        Task<Parameter?> GetByIdAsync(int id);
        Task<List<Parameter>> GetByCategoryAsync(string category);
        Task<List<string>> GetCodesByCategoryAsync(string category);
        Task<Parameter?> GetByCodeAndCategoryAsync(string category, string code);
    }
}