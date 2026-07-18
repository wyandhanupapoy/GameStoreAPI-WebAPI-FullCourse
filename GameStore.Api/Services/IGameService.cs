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

    // === New Optimized Methods ===

    /// <summary>
    /// Mengambil semua game tanpa pagination — untuk benchmark.
    /// Menggunakan optimized query (AsNoTracking + Projection).
    /// </summary>
    Task<List<GameSummaryDto>> GetAllGamesNoPaginationAsync();

    /// <summary>
    /// Versi optimized dari GetAllGamesAsync yang menggunakan LINQ
    /// projection instead of raw SQL + entity materialization.
    /// </summary>
    Task<PagedResultDto<GameSummaryDto>> GetAllGamesOptimizedAsync(GameFilterDto filter);

    /// <summary>
    /// Fuzzy search — typo tolerant search menggunakan Levenshtein + Token Matching.
    /// Contoh: "Winbing Eleven" → "Winning Eleven 2024", "Winning Eleven 2023"
    /// </summary>
    Task<List<SearchResultDto>> SearchGamesAsync(string query, double threshold = 0.6);
}
