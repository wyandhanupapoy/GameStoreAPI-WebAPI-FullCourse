using System.Linq.Expressions;
using GameStore.Api.Models;
using GameStore.Api.Dtos.Games;

namespace GameStore.Api.Repositories;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync(Expression<Func<Game, bool>>? filter = null, string? includeProperties = null);
    Task<(IEnumerable<Game> Items, int TotalCount)> GetAllWithFilterAsync(GameFilterDto filter, string? includeProperties = null);
    Task<Game?> GetAsync(Expression<Func<Game, bool>> filter, string? includeProperties = null);
    Task AddAsync(Game entity);
    void Update(Game entity);
    void Remove(Game entity);
    Task SaveAsync();

    // === Optimized Methods ===

    /// <summary>
    /// Mengambil SEMUA game tanpa pagination menggunakan:
    /// - AsNoTracking() → skip change tracking (hemat RAM)
    /// - Projection via Select() → hanya ambil kolom yang dibutuhkan
    /// - Include via Join → Genre name di-resolve di level SQL, bukan N+1
    /// 
    /// Ini menggantikan pattern: GetAllAsync() + Include("Genre") + manual mapping di Service
    /// </summary>
    Task<List<GameSummaryDto>> GetAllOptimizedAsync();

    /// <summary>
    /// Versi optimized dari GetAllWithFilterAsync.
    /// Menggunakan LINQ instead of raw SQL, dengan:
    /// - AsNoTracking()
    /// - Projection via Select()
    /// - Single query (count + data via IQueryable chaining)
    /// </summary>
    Task<(List<GameSummaryDto> Items, int TotalCount)> GetAllWithFilterOptimizedAsync(GameFilterDto filter);

    /// <summary>
    /// Mengambil semua game names saja (Id + Name) untuk fuzzy search.
    /// Sangat lightweight — hanya 2 kolom, tanpa Join.
    /// Untuk 300 data ≈ 10KB di memory.
    /// </summary>
    Task<List<(int Id, string Name)>> GetAllGameNamesAsync();

    /// <summary>
    /// Mengambil game berdasarkan list of IDs (hasil fuzzy search).
    /// Menggunakan WHERE IN (...) yang di-optimize oleh PostgreSQL.
    /// </summary>
    Task<List<GameSummaryDto>> GetByIdsAsync(List<int> ids);
}
