namespace GameStore.Api.Dtos.Genres;

public record GenreFilterDto(
    string? Search,
    int Page = 1,
    int PageSize = 10,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
);
