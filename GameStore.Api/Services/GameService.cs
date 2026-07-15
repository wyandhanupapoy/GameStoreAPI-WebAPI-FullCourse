using GameStore.Api.Dtos;
using GameStore.Api.Models;
using GameStore.Api.Repositories;
using Npgsql;

namespace GameStore.Api.Services;

public class GameService(IGameRepository gameRepository, IGenreRepository genreRepository) : IGameService
{
    public async Task<PagedResultDto<GameSummaryDto>> GetAllGamesAsync(GameFilterDto filter)
    {
        var sql = "SELECT * FROM \"Games\" WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();
        int paramIndex = 0;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql += $" AND \"Name\" ILIKE @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Search}%"));
            paramIndex++;
        }

        if (filter.MinPrice.HasValue)
        {
            sql += $" AND \"Price\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.MinPrice.Value));
            paramIndex++;
        }

        if (filter.MaxPrice.HasValue)
        {
            sql += $" AND \"Price\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.MaxPrice.Value));
            paramIndex++;
        }

        if (filter.StartDate.HasValue)
        {
            sql += $" AND \"ReleaseDate\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            paramIndex++;
        }

        if (filter.EndDate.HasValue)
        {
            sql += $" AND \"ReleaseDate\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.EndDate.Value));
            paramIndex++;
        }

        var (items, totalCount) = await gameRepository.GetAllWithRawSqlAsync(
            sql,
            parameters.ToArray(),
            filter.Page,
            filter.PageSize,
            includeProperties: "Genre");

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
        // Akses repository ke-2 (GenreRepository) untuk validasi (Membuktikan 2 repo dalam 1 service)
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

        // Akses repository pertama (GameRepository) untuk operasi inti
        await gameRepository.AddAsync(game);

        // Memanggil SaveAsync di salah satu repository
        await gameRepository.SaveAsync();

        return new GameDetailsDto(game.Id, game.Name, game.GenreId, game.Price, game.ReleaseDate, game.CreatedAt, game.UpdatedAt);
    }

    public async Task<bool> UpdateGameAsync(int id, UpdateGameDto updatedGame)
    {
        var existingGame = await gameRepository.GetAsync(g => g.Id == id);
        if (existingGame is null) return false;

        if (updatedGame.GenreId.HasValue)
        {
            // Akses repository ke-2 (GenreRepository) untuk validasi
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