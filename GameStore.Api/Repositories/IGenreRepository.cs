using System.Linq.Expressions;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Genres;

namespace GameStore.Api.Repositories;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync(Expression<Func<Genre, bool>>? filter = null, string? includeProperties = null);
    Task<(IEnumerable<Genre> Items, int TotalCount)> GetAllWithFilterAsync(GenreFilterDto filter, string? includeProperties = null);
    Task<Genre?> GetAsync(Expression<Func<Genre, bool>> filter, string? includeProperties = null);
    Task AddAsync(Genre entity);
    void Update(Genre entity);
    void Remove(Genre entity);
    Task SaveAsync();
}
