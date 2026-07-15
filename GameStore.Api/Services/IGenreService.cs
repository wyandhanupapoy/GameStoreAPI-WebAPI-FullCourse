using GameStore.Api.Dtos.Genres;

using GameStore.Api.Dtos.Common;

namespace GameStore.Api.Services;

public interface IGenreService
{
    Task<PagedResultDto<GenreDto>> GetAllGenresAsync(GenreFilterDto filter);
    Task<GenreDto?> GetGenreByIdAsync(int id);
    Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre);
    Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre);
    Task DeleteGenreAsync(int id);
}
