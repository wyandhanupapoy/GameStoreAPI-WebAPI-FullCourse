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
    public async Task<ActionResult<PagedResultDto<GenreDto>>> GetGenres([FromQuery] GenreFilterDto filter)
    {
        var result = await genreService.GetAllGenresAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = "GetGenre")]
    public async Task<ActionResult<GenreDto>> GetGenre(int id)
    {
        var genre = await genreService.GetGenreByIdAsync(id);
        if (genre is null) return NotFound();
        return Ok(genre);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GenreDto>> CreateGenre(CreateGenreDto newGenre)
    {
        var genre = await genreService.CreateGenreAsync(newGenre);
        return CreatedAtRoute("GetGenre", new { id = genre.Id }, genre);
    }

    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateGenre(int id, UpdateGenreDto updatedGenre)
    {
        var isUpdated = await genreService.UpdateGenreAsync(id, updatedGenre);
        if (!isUpdated) return NotFound();
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        await genreService.DeleteGenreAsync(id);
        return Ok();
    }
}
