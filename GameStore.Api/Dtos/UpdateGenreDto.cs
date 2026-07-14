using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record UpdateGenreDto(
    [StringLength(50)] string? Name
);
