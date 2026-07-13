using GameStore.Api.Models;

namespace GameStore.Api.Repositories;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(int id);
    Task CreateAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(int id);
}