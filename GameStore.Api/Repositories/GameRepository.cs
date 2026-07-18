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

    // ==========================================
    // === OPTIMIZED METHODS (Phase 2 & 3) ===
    // ==========================================

    /// <summary>
    /// GET ALL tanpa pagination — untuk benchmarking.
    /// 
    /// Optimasi yang diterapkan:
    /// 1. AsNoTracking() — karena ini read-only, Change Tracker tidak perlu track entity.
    ///    Dampak: hemat ~30-50% memory, query lebih cepat karena EF tidak perlu
    ///    membuat snapshot entity untuk perbandingan saat SaveChanges().
    /// 
    /// 2. Select() Projection — hanya mengambil kolom yang dibutuhkan oleh GameSummaryDto.
    ///    SQL yang dihasilkan: SELECT g."Id", g."Name", genre."Name", g."Price", ... 
    ///    BUKAN: SELECT * FROM "Games"
    ///    Dampak: mengurangi data transfer dari DB ke aplikasi.
    /// 
    /// 3. Include via Navigation Property — g.Genre!.Name di dalam Select() otomatis
    ///    membuat EF Core generate INNER JOIN, BUKAN query terpisah.
    ///    Dampak: 1 query instead of N+1 queries.
    ///    
    ///    SQL yang dihasilkan (kurang lebih):
    ///    SELECT g."Id", g."Name", g0."Name", g."Price", g."ReleaseDate", g."CreatedAt", g."UpdatedAt"
    ///    FROM "Games" AS g
    ///    INNER JOIN "Genres" AS g0 ON g."GenreId" = g0."Id"
    /// </summary>
    public async Task<List<GameSummaryDto>> GetAllOptimizedAsync()
    {
        return await _dbContext.Games
            .AsNoTracking()
            .Select(g => new GameSummaryDto(
                g.Id,
                g.Name,
                g.Genre!.Name,    // EF Core auto-generates JOIN — no N+1!
                g.Price,
                g.ReleaseDate,
                g.CreatedAt,
                g.UpdatedAt
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Versi optimized dari GetAllWithFilterAsync.
    /// 
    /// Perbedaan dari versi lama:
    /// 
    /// BEFORE (masalah):
    /// - Menggunakan FromSqlRaw() → SELECT * (semua kolom)
    /// - Include("Genre") via string → risiko typo, N+1
    /// - Materialize Game entity → map manual ke DTO di Service layer
    /// - Tidak ada AsNoTracking()
    /// 
    /// AFTER (optimized):
    /// - Menggunakan LINQ → EF Core generate SQL yang optimal
    /// - Genre name via Projection → single JOIN query
    /// - Langsung project ke DTO → skip entity materialization
    /// - AsNoTracking() → skip change tracking
    /// </summary>
    public async Task<(List<GameSummaryDto> Items, int TotalCount)> GetAllWithFilterOptimizedAsync(GameFilterDto filter)
    {
        // Start dari IQueryable — belum ada query ke DB
        IQueryable<Game> query = _dbContext.Games.AsNoTracking();

        // Apply filters menggunakan LINQ (EF Core translate ke SQL WHERE clause)
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

        // Hitung total SEBELUM pagination (tapi SESUDAH filter)
        int totalCount = await query.CountAsync();

        // Project ke DTO + pagination
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

    /// <summary>
    /// Mengambil semua game names untuk fuzzy search.
    /// 
    /// Kenapa method terpisah?
    /// - Fuzzy search hanya butuh Id dan Name untuk matching
    /// - Mengambil semua kolom (Price, ReleaseDate, dll) sia-sia
    /// - Dengan Select(g => ValueTuple), EF Core generate:
    ///   SELECT g."Id", g."Name" FROM "Games" AS g
    /// - Untuk 300 data ≈ 10KB di memory (sangat ringan)
    /// 
    /// Kenapa tidak pakai anonymous type?
    /// - ValueTuple lebih ringan (no heap allocation untuk small struct)
    /// - Cocok untuk data sementara yang langsung diproses
    /// </summary>
    public async Task<List<(int Id, string Name)>> GetAllGameNamesAsync()
    {
        return await _dbContext.Games
            .AsNoTracking()
            .Select(g => new ValueTuple<int, string>(g.Id, g.Name))
            .ToListAsync();
    }

    /// <summary>
    /// Mengambil game berdasarkan list of IDs (dari hasil fuzzy search).
    /// 
    /// EF Core translate .Contains() pada List menjadi SQL:
    ///   WHERE g."Id" IN (1, 5, 42, 43, ...)
    /// 
    /// PostgreSQL sangat optimal untuk WHERE IN dengan index pada primary key.
    /// 
    /// Kenapa tidak langsung return di FuzzySearch?
    /// - Separation of concerns: fuzzy matching di C# memory, data retrieval di DB
    /// - Kita bisa re-use method ini untuk keperluan lain
    /// </summary>
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
