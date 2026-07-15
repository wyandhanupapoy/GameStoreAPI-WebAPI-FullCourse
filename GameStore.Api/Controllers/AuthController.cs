using GameStore.Api.Dtos.Auth;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var response = await authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Username atau password salah." });
        }

        return Ok(response);
    }
}
