using GameStore.Api.Models;

namespace GameStore.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task AddUserAsync(User user);
    Task SaveAsync();
}
