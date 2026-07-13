using GameStore.Api.Dtos;

namespace GameStore.Api.Services;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllGenresAsync();
}