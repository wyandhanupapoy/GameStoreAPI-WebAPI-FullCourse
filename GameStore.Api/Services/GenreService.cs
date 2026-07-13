using GameStore.Api.Dtos;
using GameStore.Api.Repositories;

namespace GameStore.Api.Services;

public class GenreService(IGenreRepository genreRepository) : IGenreService
{
    public async Task<IEnumerable<GenreDto>> GetAllGenresAsync()
    {
        var genres = await genreRepository.GetAllAsync();
        return genres.Select(genre => new GenreDto(genre.Id, genre.Name));
    }
}