using GameStore.Api.Dtos.Games;
using GameStore.Api.Dtos.Common;

namespace GameStore.Api.Services;

public interface IGameService
{
    Task<PagedResultDto<GameSummaryDto>> GetAllGamesAsync(GameFilterDto filter, CancellationToken cancellationToken = default);
    Task<GameDetailsDto?> GetGameByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame, CancellationToken cancellationToken = default);
    Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame, CancellationToken cancellationToken = default);
    Task DeleteGameAsync(int id, CancellationToken cancellationToken = default);

    Task<List<SearchResultDto>> SearchGamesAsync(string query, double threshold = 0.6, CancellationToken cancellationToken = default);
}
