using GameStore.Api.Dtos.Genres;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

using GameStore.Api.Dtos.Common;
using Npgsql;

namespace GameStore.Api.Services;

public class GenreService(IGenreRepository genreRepository) : IGenreService
{
    public async Task<PagedResultDto<GenreDto>> GetAllGenresAsync(GenreFilterDto filter)
    {
        var sql = "SELECT * FROM \"Genres\" WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();
        int paramIndex = 0;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql += $" AND \"Name\" ILIKE @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Search}%"));
            paramIndex++;
        }

        if (filter.StartDate.HasValue)
        {
            sql += $" AND \"CreatedAt\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            paramIndex++;
        }

        if (filter.EndDate.HasValue)
        {
            sql += $" AND \"CreatedAt\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.EndDate.Value));
            paramIndex++;
        }

        var (items, totalCount) = await genreRepository.GetAllWithRawSqlAsync(
            sql,
            parameters.ToArray(),
            filter.Page,
            filter.PageSize);

        var genreDtos = items.Select(genre => new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt));

        return new PagedResultDto<GenreDto>
        {
            Items = genreDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id)
    {
        var genre = await genreRepository.GetAsync(g => g.Id == id);
        if (genre is null) return null;

        return new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt);
    }

    public async Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre)
    {
        Genre genre = new()
        {
            Name = newGenre.Name
        };

        await genreRepository.AddAsync(genre);
        await genreRepository.SaveAsync();

        return new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt);
    }

    public async Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id);
        if (existingGenre is null) return false;

        if (updatedGenre.Name != null) existingGenre.Name = updatedGenre.Name;

        genreRepository.Update(existingGenre);
        await genreRepository.SaveAsync();
        return true;
    }

    public async Task DeleteGenreAsync(int id)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id);
        if (existingGenre is not null)
        {
            genreRepository.Remove(existingGenre);
            await genreRepository.SaveAsync();
        }
    }
}
