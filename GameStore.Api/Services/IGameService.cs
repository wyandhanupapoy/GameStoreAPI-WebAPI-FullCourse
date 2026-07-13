using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IGameService
{
    Task<IEnumerable<GameSummaryDto>> GetAllGamesAsync();
    Task<GameDetailsDto?> GetGameByIdAsync(int id);
    Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame);
    Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame);
    Task DeleteGameAsync(int id);
}