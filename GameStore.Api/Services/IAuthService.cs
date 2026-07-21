using GameStore.Api.Dtos.Auth;

namespace GameStore.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
}
