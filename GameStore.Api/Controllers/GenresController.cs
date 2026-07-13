using GameStore.Api.Dtos;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Controllers;

[ApiController]
[Route("genres")]
public class GenresController(IGenreService genreService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
    {
        var genres = await genreService.GetAllGenresAsync();
        return Ok(genres);
    }
}