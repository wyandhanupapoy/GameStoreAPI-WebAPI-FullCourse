using GameStore.Api.Dtos;
using GameStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Controllers;

[ApiController]
[Route("games")]
public class GamesController(IGameService gameService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameSummaryDto>>> GetGames()
    {
        var games = await gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("{id}", Name = "GetGame")]
    public async Task<ActionResult<GameDetailsDto>> GetGame(int id)
    {
        var game = await gameService.GetGameByIdAsync(id);

        if (game is null) return NotFound();
        return Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto newGame)
    {
        var game = await gameService.CreateGameAsync(newGame);
        return CreatedAtRoute("GetGame", new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(int id, UpdateGameDto updatedGame)
    {
        var isUpdated = await gameService.UpdateGameAsync(id, updatedGame);

        if (!isUpdated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        await gameService.DeleteGameAsync(id);
        return NoContent();
    }
}