using System.Linq.Expressions;
using GameStore.Api.Data;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Games;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameStore.Api.Repositories;

public class GameRepository(GameStoreContext dbContext) : IGameRepository
{
    private readonly GameStoreContext _dbContext = dbContext;

    public async Task<IEnumerable<Game>> GetAllAsync(Expression<Func<Game, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<Game> query = _dbContext.Games;

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

    public async Task<(IEnumerable<Game> Items, int TotalCount)> GetAllWithFilterAsync(GameFilterDto filter, string? includeProperties = null)
    {
        var sql = "SELECT * FROM \"Games\" WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();
        int paramIndex = 0;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            sql += $" AND \"Name\" ILIKE @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", $"%{filter.Search}%"));
            paramIndex++;
        }

        if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
        {
            sql += $" AND \"Price\" BETWEEN @p{paramIndex} AND @p{paramIndex+1}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.MinPrice.Value));
            parameters.Add(new NpgsqlParameter($"@p{paramIndex+1}", filter.MaxPrice.Value));
            paramIndex += 2;
        }
        else if (filter.MinPrice.HasValue)
        {
            sql += $" AND \"Price\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.MinPrice.Value));
            paramIndex++;
        }
        else if (filter.MaxPrice.HasValue)
        {
            sql += $" AND \"Price\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.MaxPrice.Value));
            paramIndex++;
        }

        if (filter.StartDate.HasValue && filter.EndDate.HasValue)
        {
            sql += $" AND \"ReleaseDate\" BETWEEN @p{paramIndex} AND @p{paramIndex+1}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            parameters.Add(new NpgsqlParameter($"@p{paramIndex+1}", filter.EndDate.Value));
            paramIndex += 2;
        }
        else if (filter.StartDate.HasValue)
        {
            sql += $" AND \"ReleaseDate\" >= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.StartDate.Value));
            paramIndex++;
        }
        else if (filter.EndDate.HasValue)
        {
            sql += $" AND \"ReleaseDate\" <= @p{paramIndex}";
            parameters.Add(new NpgsqlParameter($"@p{paramIndex}", filter.EndDate.Value));
            paramIndex++;
        }

        IQueryable<Game> query = _dbContext.Games.FromSqlRaw(sql, parameters.ToArray());
        
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

    public async Task<Game?> GetAsync(Expression<Func<Game, bool>> filter, string? includeProperties = null)
    {
        IQueryable<Game> query = _dbContext.Games;
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

    public async Task AddAsync(Game entity)
    {
        await _dbContext.Games.AddAsync(entity);
    }

    public void Remove(Game entity)
    {
        _dbContext.Games.Remove(entity);
    }

    public void Update(Game entity)
    {
        _dbContext.Games.Update(entity);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
