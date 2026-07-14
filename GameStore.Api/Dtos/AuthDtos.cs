using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record RegisterDto(
    [Required][StringLength(50)] string Username,
    [Required][StringLength(100, MinimumLength = 6)] string Password
);

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Role
);
