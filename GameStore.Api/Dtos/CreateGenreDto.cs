using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record CreateGenreDto(
    [Required][StringLength(50)] string Name
);
