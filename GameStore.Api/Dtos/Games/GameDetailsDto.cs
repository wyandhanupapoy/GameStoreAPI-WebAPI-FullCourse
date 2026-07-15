namespace GameStore.Api.Dtos.Games;

public record GameDetailsDto(
    int Id,
    string Name,
    int GenreId,
    decimal Price,
    DateOnly ReleaseDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
