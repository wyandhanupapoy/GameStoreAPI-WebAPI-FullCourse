using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos.Games;

public record UpdateGameDto(
    [StringLength(50)] string? Name,
    [Range(1, 50)] int? GenreId,
    [Range(1, 100)] decimal? Price,
    DateOnly? ReleaseDate
);
