using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto request);
    Task<AuthResponseDto?> LoginAsync(LoginDto request);
}
