using bancoSol.DTOs;

namespace bancoSol.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateResponse> GetRateAsync();
    }
}