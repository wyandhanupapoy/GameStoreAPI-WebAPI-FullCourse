using GameStore.Api.Dtos;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GameService(IGameRepository gameRepository) : IGameService
{
    public async Task<IEnumerable<GameSummaryDto>> GetAllGamesAsync()
    {
        var games = await gameRepository.GetAllAsync();
        return games.Select(game => new GameSummaryDto(
            game.Id,
            game.Name,
            game.Genre!.Name,
            game.Price,
            game.ReleaseDate
        ));
    }

    public async Task<GameDetailsDto?> GetGameByIdAsync(int id)
    {
        var game = await gameRepository.GetByIdAsync(id);
        if (game is null) return null;

        return new GameDetailsDto(
            game.Id,
            game.Name,
            game.GenreId,
            game.Price,
            game.ReleaseDate
        );
    }

    public async Task<GameDetailsDto> CreateGameAsync(CreateGameDto newGame)
    {
        Game game = new()
        {
            Name = newGame.Name,
            GenreId = newGame.GenreId,
            Price = newGame.Price,
            ReleaseDate = newGame.ReleaseDate
        };

        await gameRepository.CreateAsync(game);

        return new GameDetailsDto(game.Id, game.Name, game.GenreId, game.Price, game.ReleaseDate);
    }

    public async Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame)
    {
        var existingGame = await gameRepository.GetByIdAsync(id);
        if (existingGame is null) return false;

        existingGame.Name = updatedGame.Name;
        existingGame.GenreId = updatedGame.GenreId;
        existingGame.Price = updatedGame.Price;
        existingGame.ReleaseDate = updatedGame.ReleaseDate;

        await gameRepository.UpdateAsync(existingGame);
        return true;
    }

    public async Task DeleteGameAsync(int id)
    {
        await gameRepository.DeleteAsync(id);
    }
}