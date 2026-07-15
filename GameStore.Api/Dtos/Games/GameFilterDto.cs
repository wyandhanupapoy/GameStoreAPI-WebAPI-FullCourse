namespace GameStore.Api.Dtos.Games;

public record GameFilterDto(
    string? Search,
    int Page = 1,
    int PageSize = 10,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null
);
