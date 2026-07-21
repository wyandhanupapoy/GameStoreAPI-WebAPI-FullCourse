using GameStore.Api.Models;

namespace GameStore.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
