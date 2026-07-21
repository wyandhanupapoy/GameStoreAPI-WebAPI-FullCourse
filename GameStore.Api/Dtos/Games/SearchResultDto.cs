namespace GameStore.Api.Dtos.Games;

public record SearchResultDto(
    int Id,
    string Name,
    string Genre,
    decimal Price,
    DateOnly ReleaseDate,
    double RelevanceScore
);
