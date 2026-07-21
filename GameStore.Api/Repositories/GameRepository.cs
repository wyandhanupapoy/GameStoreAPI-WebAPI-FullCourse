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

    public async Task<(List<GameSummaryDto> Items, int TotalCount)> GetAllWithFilterAsync(GameFilterDto filter)
    {
        IQueryable<Game> query = _dbContext.Games.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string searchLower = filter.Search.ToLower();
            query = query.Where(g => g.Name.ToLower().Contains(searchLower));
        }

        if (filter.MinPrice.HasValue)
            query = query.Where(g => g.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(g => g.Price <= filter.MaxPrice.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(g => g.ReleaseDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(g => g.ReleaseDate <= filter.EndDate.Value);

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(g => g.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(g => new GameSummaryDto(
                g.Id,
                g.Name,
                g.Genre!.Name,
                g.Price,
                g.ReleaseDate,
                g.CreatedAt,
                g.UpdatedAt
            ))
            .ToListAsync();

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



    public async Task<List<(int Id, string Name)>> GetAllGameNamesAsync()
    {
        return await _dbContext.Games
            .AsNoTracking()
            .Select(g => new ValueTuple<int, string>(g.Id, g.Name))
            .ToListAsync();
    }

    public async Task<List<GameSummaryDto>> GetByIdsAsync(List<int> ids)
    {
        return await _dbContext.Games
            .AsNoTracking()
            .Where(g => ids.Contains(g.Id))
            .Select(g => new GameSummaryDto(
                g.Id,
                g.Name,
                g.Genre!.Name,
                g.Price,
                g.ReleaseDate,
                g.CreatedAt,
                g.UpdatedAt
            ))
            .ToListAsync();
    }
}
