using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Repositories;

public class GenreRepository(GameStoreContext dbContext) : IGenreRepository
{
    public async Task<IEnumerable<Genre>> GetAllAsync()
    {
        return await dbContext.Genres.AsNoTracking().ToListAsync();
    }
}