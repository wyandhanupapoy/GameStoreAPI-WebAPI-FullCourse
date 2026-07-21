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
    public async Task<ActionResult<PagedResultDto<GameSummaryDto>>> GetGames([FromQuery] GameFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await gameService.GetAllGamesAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<SearchResultDto>>> SearchGames(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] double threshold = 0.6,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query parameter 'q' is required." });

        var results = await gameService.SearchGamesAsync(query, threshold, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id:int}", Name = "GetGame")]
    public async Task<ActionResult<GameDetailsDto>> GetGame(int id, CancellationToken cancellationToken)
    {
        var game = await gameService.GetGameByIdAsync(id, cancellationToken);

        if (game is null) return NotFound();
        return Ok(game);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto newGame, CancellationToken cancellationToken)
    {
        var game = await gameService.CreateGameAsync(newGame, cancellationToken);
        return CreatedAtRoute("GetGame", new { id = game.Id }, game);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateGame(int id, UpdateGameDto updatedGame, CancellationToken cancellationToken)
    {
        var isUpdated = await gameService.UpdateGameAsync(id, updatedGame, cancellationToken);

        if (!isUpdated) return NotFound();
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGame(int id, CancellationToken cancellationToken)
    {
        await gameService.DeleteGameAsync(id, cancellationToken);
        return Ok();
    }
}
