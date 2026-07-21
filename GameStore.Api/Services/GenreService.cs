using GameStore.Api.Dtos.Common;
using GameStore.Api.Dtos.Genres;
using GameStore.Api.Models;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GenreService(IGenreRepository genreRepository) : IGenreService
{
    public async Task<PagedResultDto<GenreDto>> GetAllGenresAsync(GenreFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await genreRepository.GetAllWithFilterAsync(filter, null, cancellationToken);

        var genreDtos = items.Select(genre => new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt));

        return new PagedResultDto<GenreDto>
        {
            Items = genreDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var genre = await genreRepository.GetAsync(g => g.Id == id, null, cancellationToken);
        if (genre is null) return null;

        return new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt);
    }

    public async Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre, CancellationToken cancellationToken = default)
    {
        Genre genre = new()
        {
            Name = newGenre.Name
        };

        await genreRepository.AddAsync(genre, cancellationToken);
        await genreRepository.SaveAsync(cancellationToken);

        return new GenreDto(genre.Id, genre.Name, genre.CreatedAt, genre.UpdatedAt);
    }

    public async Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre, CancellationToken cancellationToken = default)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id, null, cancellationToken);
        if (existingGenre is null) return false;

        existingGenre.Name = updatedGenre.Name;

        genreRepository.Update(existingGenre);
        await genreRepository.SaveAsync(cancellationToken);
        return true;
    }

    public async Task DeleteGenreAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingGenre = await genreRepository.GetAsync(g => g.Id == id, null, cancellationToken);
        if (existingGenre is not null)
        {
            genreRepository.Remove(existingGenre);
            await genreRepository.SaveAsync(cancellationToken);
        }
    }
}
