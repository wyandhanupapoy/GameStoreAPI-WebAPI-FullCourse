using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameStore.Api.Dtos.Auth;
using GameStore.Api.Models;
using GameStore.Api.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.Api.Services;

public class AuthService(IUserRepository userRepository, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetUserByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new ArgumentException("Username sudah digunakan.");
        }

        // Hash password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User"
        };

        await userRepository.AddUserAsync(newUser, cancellationToken);
        await userRepository.SaveAsync(cancellationToken);

        var token = GenerateJwtToken(newUser);
        return new AuthResponseDto(token, newUser.Username, newUser.Role);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetUserByUsernameAsync(request.Username, cancellationToken);
        
        if (user == null)
        {
            return null;
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        return new AuthResponseDto(token, user.Username, user.Role);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing.")));
            
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
