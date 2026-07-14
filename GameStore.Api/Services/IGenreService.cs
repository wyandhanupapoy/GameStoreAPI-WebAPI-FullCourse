using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllGenresAsync();
    Task<GenreDto?> GetGenreByIdAsync(int id);
    Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre);
    Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre);
    Task DeleteGenreAsync(int id);
}