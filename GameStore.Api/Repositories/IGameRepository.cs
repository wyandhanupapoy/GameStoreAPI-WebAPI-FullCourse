using System.Linq.Expressions;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Games;

namespace GameStore.Api.Repositories;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync(Expression<Func<Game, bool>>? filter = null, string? includeProperties = null, CancellationToken cancellationToken = default);
    Task<(List<GameSummaryDto> Items, int TotalCount)> GetAllWithFilterAsync(GameFilterDto filter, CancellationToken cancellationToken = default);
    Task<Game?> GetAsync(Expression<Func<Game, bool>> filter, string? includeProperties = null, CancellationToken cancellationToken = default);
    Task AddAsync(Game entity, CancellationToken cancellationToken = default);
    void Update(Game entity);
    void Remove(Game entity);
    Task SaveAsync(CancellationToken cancellationToken = default);

    Task<List<(int Id, string Name)>> GetAllGameNamesAsync(CancellationToken cancellationToken = default);

    Task<List<GameSummaryDto>> GetByIdsAsync(List<int> ids, CancellationToken cancellationToken = default);
}
