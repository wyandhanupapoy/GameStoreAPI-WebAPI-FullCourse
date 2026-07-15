using System.Linq.Expressions;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Games;

namespace GameStore.Api.Repositories;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync(Expression<Func<Game, bool>>? filter = null, string? includeProperties = null);
    Task<(IEnumerable<Game> Items, int TotalCount)> GetAllWithFilterAsync(GameFilterDto filter, string? includeProperties = null);
    Task<Game?> GetAsync(Expression<Func<Game, bool>> filter, string? includeProperties = null);
    Task AddAsync(Game entity);
    void Update(Game entity);
    void Remove(Game entity);
    Task SaveAsync();
}
