using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MarketMindAPI.Models;

namespace MarketMindAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritesController(AppDbContext db)
    {
        _db = db;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Kullanıcının favori hisselerini listele</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var favorites = await _db.Favorites
            .Where(f => f.UserId == UserId)
            .OrderByDescending(f => f.AddedAt)
            .ToListAsync();
        return Ok(favorites);
    }

    /// <summary>Favoriye hisse ekle</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddFavoriteDto dto)
    {
        var exists = await _db.Favorites.AnyAsync(f => f.UserId == UserId && f.Symbol == dto.Symbol);
        if (exists) return Conflict(new { error = "Bu hisse zaten favorilerde." });

        var favorite = new Favorite
        {
            UserId = UserId,
            Symbol = dto.Symbol.ToUpper(),
            Name = dto.Name ?? dto.Symbol.ToUpper(),
            AddedAt = DateTime.UtcNow
        };

        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();
        return Ok(favorite);
    }

    /// <summary>Favoriden hisse kaldır</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var favorite = await _db.Favorites.FindAsync(id);
        if (favorite == null) return NotFound();
        if (favorite.UserId != UserId) return Forbid();

        _db.Favorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Silindi." });
    }
}

public record AddFavoriteDto(string Symbol, string? Name);
