using System.Linq.Expressions;
using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Repositories;

public class GenreRepository(GameStoreContext dbContext) : IGenreRepository
{
    private readonly GameStoreContext _dbContext = dbContext;

    public async Task<IEnumerable<Genre>> GetAllAsync(Expression<Func<Genre, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<Genre> query = _dbContext.Genres;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
        }

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Genre> Items, int TotalCount)> GetAllWithRawSqlAsync(string sql, object[] parameters, int page, int pageSize, string? includeProperties = null)
    {
        IQueryable<Genre> query = _dbContext.Genres.FromSqlRaw(sql, parameters);
        
        int totalCount = await query.CountAsync();
        
        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
        }
        
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<Genre?> GetAsync(Expression<Func<Genre, bool>> filter, string? includeProperties = null)
    {
        IQueryable<Genre> query = _dbContext.Genres;
        query = query.Where(filter);

        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(Genre entity)
    {
        await _dbContext.Genres.AddAsync(entity);
    }

    public void Remove(Genre entity)
    {
        _dbContext.Genres.Remove(entity);
    }

    public void Update(Genre entity)
    {
        _dbContext.Genres.Update(entity);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
