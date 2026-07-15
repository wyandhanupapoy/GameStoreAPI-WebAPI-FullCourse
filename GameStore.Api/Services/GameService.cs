using GameStore.Api.Dtos.Games;
using GameStore.Api.Dtos.Common;
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
}
