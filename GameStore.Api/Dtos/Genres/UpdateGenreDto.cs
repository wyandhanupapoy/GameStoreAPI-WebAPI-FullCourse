using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos.Genres;

public record UpdateGenreDto(
    [StringLength(50)] string? Name
);
