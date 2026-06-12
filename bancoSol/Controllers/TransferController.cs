using bancoSol.DTOs;
using bancoSol.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bancoSol.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(TransferResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
        {
            var result = await _transferService.CreateTransferAsync(request);
            return Ok(result);
        }
    }
}