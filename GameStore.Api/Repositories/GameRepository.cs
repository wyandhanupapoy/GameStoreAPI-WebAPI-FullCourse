using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Repositories;

public class GameRepository(GameStoreContext dbContext) : IGameRepository
{
    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await dbContext.Games.Include(g => g.Genre).AsNoTracking().ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await dbContext.Games.FindAsync(id);
    }

    public async Task CreateAsync(Game game)
    {
        dbContext.Games.Add(game);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        dbContext.Games.Update(game);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await dbContext.Games.Where(game => game.Id == id).ExecuteDeleteAsync();
    }
}