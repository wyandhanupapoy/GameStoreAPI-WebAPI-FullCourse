using System.Linq.Expressions;
using GameStore.Api.Data;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Genres;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task<(IEnumerable<Genre> Items, int TotalCount)> GetAllWithFilterAsync(GenreFilterDto filter, string? includeProperties = null)
    {
        var sql = "SELECT * FROM \"Genres\" WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();
        int paramIndex = 0;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql += $" AND \"Name\" ILIKE @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Search}%"));
            paramIndex++;
        }

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            sql += $" AND \"CreatedAt\" BETWEEN @p{paramIndex} AND @p{paramIndex+1}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            parameters.Add(new NpgsqlParameter($"@p{paramIndex+1}", filter.EndDate.Value));
            paramIndex += 2;
        }
        else if (filter.StartDate.HasValue)
        {
            sql += $" AND \"CreatedAt\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            paramIndex++;
        }
        else if (filter.EndDate.HasValue)
        {
            sql += $" AND \"CreatedAt\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.EndDate.Value));
            paramIndex++;
        }

        IQueryable<Genre> query = _dbContext.Genres.FromSqlRaw(sql, parameters.ToArray());
        
        int totalCount = await query.CountAsync();
        
        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
        }
        
        var items = await query.OrderBy(g => g.Id).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        
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
