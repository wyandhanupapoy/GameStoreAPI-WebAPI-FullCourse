namespace GameStore.Api.Dtos.Games;

public record GameSummaryDto(
    int Id,
    string Name,
    string Genre,
    decimal Price,
    DateOnly ReleaseDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
