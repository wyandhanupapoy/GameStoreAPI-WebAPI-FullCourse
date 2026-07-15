namespace GameStore.Api.Dtos;

public record GameSummaryDto(
    int Id,
    string Name,
    string Genre,
    decimal Price,
    DateOnly ReleaseDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);