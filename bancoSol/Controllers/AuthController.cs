using bancoSol.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace bancoSol.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(ITokenService tokenService, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var validUsername = _configuration["AdminCredentials:Username"];
        var validPassword = _configuration["AdminCredentials:Password"];

        if (request.Username == validUsername && request.Password == validPassword)
        {
            var token = _tokenService.GenerateToken(request.Username, "Admin");
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Credenciales inválidas" });
    }
}

public record LoginRequest(string Username, string Password);