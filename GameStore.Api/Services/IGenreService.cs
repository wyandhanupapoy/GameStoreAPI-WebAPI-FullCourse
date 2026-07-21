using GameStore.Api.Dtos.Genres;

using GameStore.Api.Dtos.Common;

namespace GameStore.Api.Services;

public interface IGenreService
{
    Task<PagedResultDto<GenreDto>> GetAllGenresAsync(GenreFilterDto filter, CancellationToken cancellationToken = default);
    Task<GenreDto?> GetGenreByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GenreDto> CreateGenreAsync(CreateGenreDto newGenre, CancellationToken cancellationToken = default);
    Task<bool> UpdateGenreAsync(int id, UpdateGenreDto updatedGenre, CancellationToken cancellationToken = default);
    Task DeleteGenreAsync(int id, CancellationToken cancellationToken = default);
}
