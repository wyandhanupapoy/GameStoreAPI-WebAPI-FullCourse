using GameStore.Api.Dtos.Common;
using GameStore.Api.Dtos.Games;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Controllers;

[ApiController]
[Route("games")]
public class GamesController(IGameService gameService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<GameSummaryDto>>> GetGames([FromQuery] GameFilterDto filter)
    {
        var result = await gameService.GetAllGamesAsync(filter);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<GameSummaryDto>>> GetAllGames()
    {
        var result = await gameService.GetAllGamesNoPaginationAsync();
        return Ok(result);
    }

    [HttpGet("optimized")]
    public async Task<ActionResult<PagedResultDto<GameSummaryDto>>> GetGamesOptimized([FromQuery] GameFilterDto filter)
    {
        var result = await gameService.GetAllGamesOptimizedAsync(filter);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<SearchResultDto>>> SearchGames(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] double threshold = 0.6)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query parameter 'q' is required." });

        var results = await gameService.SearchGamesAsync(query, threshold);
        return Ok(results);
    }

    [HttpGet("{id:int}", Name = "GetGame")]
    public async Task<ActionResult<GameDetailsDto>> GetGame(int id)
    {
        var game = await gameService.GetGameByIdAsync(id);

        if (game is null) return NotFound();
        return Ok(game);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto newGame)
    {
        var game = await gameService.CreateGameAsync(newGame);
        return CreatedAtRoute("GetGame", new { id = game.Id }, game);
    }

    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateGame(int id, UpdateGameDto updatedGame)
    {
        var isUpdated = await gameService.UpdateGameAsync(id, updatedGame);

        if (!isUpdated) return NotFound();
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        await gameService.DeleteGameAsync(id);
        return Ok();
    }
}
