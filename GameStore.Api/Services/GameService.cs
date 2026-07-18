using GameStore.Api.Dtos.Games;
using GameStore.Api.Dtos.Common;
using GameStore.Api.Helpers;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GameService(IGameRepository gameRepository, IGenreRepository genreRepository) : IGameService
{
    public async Task<PagedResultDto<GameSummaryDto>> GetAllGamesAsync(GameFilterDto filter)
    {
        var (items, totalCount) = await gameRepository.GetAllWithFilterAsync(filter, includeProperties: "Genre");

        var summaryItems = items.Select(game => new GameSummaryDto(
            game.Id,
            game.Name,
            game.Genre?.Name ?? string.Empty,
            game.Price,
            game.ReleaseDate,
            game.CreatedAt,
            game.UpdatedAt
        ));

        return new PagedResultDto<GameSummaryDto>
        {
            Items = summaryItems,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<GameDetailsDto?> GetGameByIdAsync(int id)
    {
        var game = await gameRepository.GetAsync(g => g.Id == id);
        if (game is null) return null;

        return new GameDetailsDto(
            game.Id,
            game.Name,
            game.GenreId,
            game.Price,
            game.ReleaseDate,
            game.CreatedAt,
            game.UpdatedAt
        );
    }

    public async Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame)
    {
        var genre = await genreRepository.GetAsync(g => g.Id == newGame.GenreId);
        if (genre is null)
        {
            throw new ArgumentException($"Genre dengan ID {newGame.GenreId} tidak ditemukan.");
        }

        Game game = new()
        {
            Name = newGame.Name,
            GenreId = newGame.GenreId,
            Price = newGame.Price,
            ReleaseDate = newGame.ReleaseDate
        };

        await gameRepository.AddAsync(game);
        await gameRepository.SaveAsync();

        return new GameDetailsDto(game.Id, game.Name, game.GenreId, game.Price, game.ReleaseDate, game.CreatedAt, game.UpdatedAt);
    }

    public async Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame)
    {
        var existingGame = await gameRepository.GetAsync(g => g.Id == id);
        if (existingGame is null) return false;

        if (updatedGame.GenreId.HasValue)
        {
            var genre = await genreRepository.GetAsync(g => g.Id == updatedGame.GenreId.Value);
            if (genre is null)
            {
                throw new ArgumentException($"Genre dengan ID {updatedGame.GenreId.Value} tidak ditemukan.");
            }
            existingGame.GenreId = updatedGame.GenreId.Value;
        }

        if (updatedGame.Name != null) existingGame.Name = updatedGame.Name;
        if (updatedGame.Price.HasValue) existingGame.Price = updatedGame.Price.Value;
        if (updatedGame.ReleaseDate.HasValue) existingGame.ReleaseDate = updatedGame.ReleaseDate.Value;

        gameRepository.Update(existingGame);
        await gameRepository.SaveAsync();
        return true;
    }

    public async Task DeleteGameAsync(int id)
    {
        var existingGame = await gameRepository.GetAsync(g => g.Id == id);
        if (existingGame is not null)
        {
            gameRepository.Remove(existingGame);
            await gameRepository.SaveAsync();
        }
    }

    // ==========================================
    // === NEW OPTIMIZED METHODS ===
    // ==========================================

    /// <summary>
    /// Mengambil semua game tanpa pagination — untuk benchmark.
    /// 
    /// Perbandingan dengan GetAllGamesAsync:
    /// 
    /// GetAllGamesAsync (BEFORE):
    /// 1. Repository: SELECT * FROM Games → materialize ke List<Game>
    /// 2. Repository: Include("Genre") → mungkin N+1 query
    /// 3. Service: .Select() di memory → map Game entity ke GameSummaryDto
    /// 4. Total: 2+ DB queries, full entity materialization, change tracking aktif
    /// 
    /// GetAllGamesNoPaginationAsync (AFTER):
    /// 1. Repository: SELECT Id, Name, Genre.Name, Price, ... FROM Games JOIN Genres
    /// 2. Langsung project ke GameSummaryDto di level DB
    /// 3. Total: 1 DB query, no entity materialization, no change tracking
    /// </summary>
    public async Task<List<GameSummaryDto>> GetAllGamesNoPaginationAsync()
    {
        return await gameRepository.GetAllOptimizedAsync();
    }

    /// <summary>
    /// Versi optimized dari GetAllGamesAsync.
    /// Menggunakan LINQ projection instead of raw SQL + entity materialization.
    /// </summary>
    public async Task<PagedResultDto<GameSummaryDto>> GetAllGamesOptimizedAsync(GameFilterDto filter)
    {
        var (items, totalCount) = await gameRepository.GetAllWithFilterOptimizedAsync(filter);

        return new PagedResultDto<GameSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Fuzzy search menggunakan Levenshtein Distance + Token Matching.
    /// 
    /// Flow:
    /// 1. Ambil semua game names dari DB (lightweight: hanya Id + Name)
    ///    SQL: SELECT "Id", "Name" FROM "Games"
    ///    Untuk 300 data ≈ 10KB, sangat cepat
    /// 
    /// 2. Jalankan FuzzySearchHelper.FuzzySearch() di C# memory
    ///    - Setiap game name di-tokenize dan dicocokkan dengan query
    ///    - Menggunakan Levenshtein Distance untuk menghitung similarity per token
    ///    - Filter hasil dengan threshold (default 60%)
    ///    - Sort by relevance score descending
    /// 
    /// 3. Ambil data lengkap dari DB untuk game yang lolos threshold
    ///    SQL: SELECT ... FROM "Games" JOIN "Genres" WHERE "Id" IN (1, 5, 42, ...)
    /// 
    /// 4. Gabungkan data lengkap dengan relevance score
    /// 
    /// Contoh:
    /// Input: "Winbing Eleven"
    /// Step 2: [(41, "Winning Eleven 2024", 0.928), (42, "Winning Eleven 2023", 0.928), ...]
    /// Step 3: Ambil detail game id 41, 42, ...
    /// Output: SearchResultDto dengan nama, genre, harga, dan score
    /// </summary>
    public async Task<List<SearchResultDto>> SearchGamesAsync(string query, double threshold = 0.6)
    {
        // Step 1: Ambil semua game names (lightweight query)
        var gameNames = await gameRepository.GetAllGameNamesAsync();

        // Step 2: Fuzzy matching di C# memory
        var fuzzyResults = FuzzySearchHelper.FuzzySearch(query, gameNames, threshold);

        if (fuzzyResults.Count == 0)
            return [];

        // Step 3: Ambil data lengkap dari DB berdasarkan matched IDs
        var matchedIds = fuzzyResults.Select(r => r.Id).ToList();
        var gameDetails = await gameRepository.GetByIdsAsync(matchedIds);

        // Step 4: Gabungkan data lengkap dengan relevance score
        // Gunakan Dictionary untuk O(1) lookup by Id
        var scoreMap = fuzzyResults.ToDictionary(r => r.Id, r => r.Score);

        return gameDetails
            .Select(g => new SearchResultDto(
                g.Id,
                g.Name,
                g.Genre,
                g.Price,
                g.ReleaseDate,
                scoreMap.GetValueOrDefault(g.Id, 0.0)
            ))
            .OrderByDescending(r => r.RelevanceScore)
            .ThenBy(r => r.Name)
            .ToList();
    }
}
