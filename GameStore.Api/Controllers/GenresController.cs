using GameStore.Api.Dtos.Common;
using GameStore.Api.Dtos.Genres;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Controllers;

[ApiController]
[Route("genres")]
public class GenresController(IGenreService genreService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<GenreDto>>> GetGenres([FromQuery] GenreFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await genreService.GetAllGenresAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = "GetGenre")]
    public async Task<ActionResult<GenreDto>> GetGenre(int id, CancellationToken cancellationToken)
    {
        var genre = await genreService.GetGenreByIdAsync(id, cancellationToken);
        if (genre is null) return NotFound();
        return Ok(genre);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<GenreDto>> CreateGenre(CreateGenreDto newGenre, CancellationToken cancellationToken)
    {
        var genre = await genreService.CreateGenreAsync(newGenre, cancellationToken);
        return CreatedAtRoute("GetGenre", new { id = genre.Id }, genre);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateGenre(int id, UpdateGenreDto updatedGenre, CancellationToken cancellationToken)
    {
        var isUpdated = await genreService.UpdateGenreAsync(id, updatedGenre, cancellationToken);
        if (!isUpdated) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGenre(int id, CancellationToken cancellationToken)
    {
        await genreService.DeleteGenreAsync(id, cancellationToken);
        return Ok();
    }
}
