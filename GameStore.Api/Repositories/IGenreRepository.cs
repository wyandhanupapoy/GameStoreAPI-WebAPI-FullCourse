using System.Linq.Expressions;
using GameStore.Api.Models;

namespace GameStore.Api.Repositories;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync(Expression<Func<Genre, bool>>? filter = null, string? includeProperties = null);
    Task<Genre?> GetAsync(Expression<Func<Genre, bool>> filter, string? includeProperties = null);
    Task AddAsync(Genre entity);
    void Update(Genre entity);
    void Remove(Genre entity);
    Task SaveAsync();
}
