using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Repositories;

public class UserRepository(GameStoreContext dbContext) : IUserRepository
{
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task AddUserAsync(User user)
    {
        await dbContext.Users.AddAsync(user);
    }

    public async Task SaveAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
