using bancoSol.DTOs;

namespace bancoSol.Services.Interfaces
{
    public interface ITransferService
    {
        Task<TransferResponse> CreateTransferAsync(CreateTransferRequest request);
    }
}