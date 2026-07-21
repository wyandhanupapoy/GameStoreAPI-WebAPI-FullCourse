using GameStore.Api.Dtos.Games;
using GameStore.Api.Dtos.Common;

namespace GameStore.Api.Services;

public interface IGameService
{
    Task<PagedResultDto<GameSummaryDto>> GetAllGamesAsync(GameFilterDto filter);
    Task<GameDetailsDto?> GetGameByIdAsync(int id);
    Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame);
    Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame);
    Task DeleteGameAsync(int id);

    Task<List<SearchResultDto>> SearchGamesAsync(string query, double threshold = 0.6);
}
