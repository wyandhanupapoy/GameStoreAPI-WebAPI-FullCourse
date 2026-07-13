using GameStore.Api.Models;

namespace GameStore.Api.Repositories;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync();
}