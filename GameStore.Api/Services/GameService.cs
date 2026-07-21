using GameStore.Api.Dtos.Common;
using GameStore.Api.Dtos.Games;
using GameStore.Api.Helpers;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GameService(IGameRepository gameRepository, IGenreRepository genreRepository) : IGameService
{
    public async Task<PagedResultDto<GameSummaryDto>> GetAllGamesAsync(GameFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await gameRepository.GetAllWithFilterAsync(filter, cancellationToken);

        return new PagedResultDto<GameSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<GameDetailsDto?> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetAsync(g => g.Id == id, null, cancellationToken);
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

    public async Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame, CancellationToken cancellationToken = default)
    {
        var genre = await genreRepository.GetAsync(g => g.Id == newGame.GenreId, null, cancellationToken);
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

        await gameRepository.AddAsync(game, cancellationToken);
        await gameRepository.SaveAsync(cancellationToken);

        return new GameDetailsDto(game.Id, game.Name, game.GenreId, game.Price, game.ReleaseDate, game.CreatedAt, game.UpdatedAt);
    }

    public async Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame, CancellationToken cancellationToken = default)
    {
        var existingGame = await gameRepository.GetAsync(g => g.Id == id, null, cancellationToken);
        if (existingGame is null) return false;

        if (updatedGame.GenreId.HasValue)
        {
            var genre = await genreRepository.GetAsync(g => g.Id == updatedGame.GenreId.Value, null, cancellationToken);
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
        await gameRepository.SaveAsync(cancellationToken);
        return true;
    }

    public async Task DeleteGameAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingGame = await gameRepository.GetAsync(g => g.Id == id, null, cancellationToken);
        if (existingGame is not null)
        {
            gameRepository.Remove(existingGame);
            await gameRepository.SaveAsync(cancellationToken);
        }
    }


    public async Task<List<SearchResultDto>> SearchGamesAsync(string query, double threshold = 0.6, CancellationToken cancellationToken = default)
    {
        var gameNames = await gameRepository.GetAllGameNamesAsync(cancellationToken);

        var fuzzyResults = FuzzySearchHelper.FuzzySearch(query, gameNames, threshold);

        if (fuzzyResults.Count == 0)
            return [];

        var matchedIds = fuzzyResults.Select(r => r.Id).ToList();
        var gameDetails = await gameRepository.GetByIdsAsync(matchedIds, cancellationToken);

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
