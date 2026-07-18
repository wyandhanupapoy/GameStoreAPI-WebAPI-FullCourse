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

    /// <summary>
    /// Endpoint untuk benchmark — mengembalikan SEMUA game tanpa pagination.
    /// Menggunakan optimized query (AsNoTracking + Projection).
    /// 
    /// Bandingkan response time endpoint ini dengan GET /games (paginated)
    /// menggunakan header X-Response-Time-Ms di response.
    /// 
    /// ⚠️ HANYA UNTUK BENCHMARK — di production, selalu gunakan pagination.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<GameSummaryDto>>> GetAllGames()
    {
        var result = await gameService.GetAllGamesNoPaginationAsync();
        return Ok(result);
    }

    /// <summary>
    /// Endpoint optimized — versi optimasi dari GET /games.
    /// Menggunakan LINQ Projection, AsNoTracking, dan single query.
    /// 
    /// Bandingkan response time endpoint ini dengan GET /games (original)
    /// untuk melihat perbedaan performa.
    /// </summary>
    [HttpGet("optimized")]
    public async Task<ActionResult<PagedResultDto<GameSummaryDto>>> GetGamesOptimized([FromQuery] GameFilterDto filter)
    {
        var result = await gameService.GetAllGamesOptimizedAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Fuzzy search endpoint — mendukung typo tolerance.
    /// 
    /// Contoh penggunaan:
    /// GET /games/search?q=Winbing+Eleven         → "Winning Eleven 2024", "Winning Eleven 2023"
    /// GET /games/search?q=Fial+Fantsy             → "Final Fantasy VII", "Final Fantasy XVI", ...
    /// GET /games/search?q=Grand+Thef+Autto        → "Grand Theft Auto V", ...
    /// GET /games/search?q=residen+evil             → "Resident Evil 4 Remake", ...
    /// 
    /// Parameter:
    /// - q: search keyword (required)
    /// - threshold: minimum similarity score 0.0-1.0 (optional, default 0.6)
    ///   Semakin rendah → semakin banyak hasil tapi kurang relevan
    ///   Semakin tinggi → semakin sedikit hasil tapi lebih relevan
    /// </summary>
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
