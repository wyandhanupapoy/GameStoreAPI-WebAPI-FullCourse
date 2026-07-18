namespace GameStore.Api.Dtos.Games;

/// <summary>
/// DTO untuk hasil fuzzy search.
/// Berbeda dari GameSummaryDto karena menyertakan RelevanceScore
/// untuk menunjukkan seberapa relevan hasil dengan keyword pencarian.
/// 
/// RelevanceScore: 0.0 - 1.0
/// - 1.0 = exact match
/// - 0.8+ = sangat mirip (typo ringan)
/// - 0.6 - 0.8 = cukup mirip (typo berat)
/// - &lt; 0.6 = tidak ditampilkan (di bawah threshold)
/// </summary>
public record SearchResultDto(
    int Id,
    string Name,
    string Genre,
    decimal Price,
    DateOnly ReleaseDate,
    double RelevanceScore
);
