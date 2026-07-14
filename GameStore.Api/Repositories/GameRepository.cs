using System.Linq.Expressions;
using GameStore.Api.Data;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<(IEnumerable<Game> Items, int TotalCount)> GetAllWithRawSqlAsync(string sql, object[] parameters, int page, int pageSize, string? includeProperties = null)
    {
        IQueryable<Game> query = _dbContext.Games.FromSqlRaw(sql, parameters);
        
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
