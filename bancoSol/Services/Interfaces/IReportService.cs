using bancoSol.DTOs;

namespace bancoSol.Services.Interfaces
{
    public interface IReportService
    {
        Task<ConsolidatedBalanceResponse> GetConsolidatedBalanceAsync(ConsolidatedBalanceRequest request);
    }
}