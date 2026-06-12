using bancoSol.DTOs;
using bancoSol.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bancoSol.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("consolidated-balance")]
        [ProducesResponseType(typeof(ConsolidatedBalanceResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetConsolidatedBalance(
            [FromQuery] ConsolidatedBalanceRequest request)
        {
            var result = await _reportService.GetConsolidatedBalanceAsync(request);
            return Ok(result);
        }
    }
}