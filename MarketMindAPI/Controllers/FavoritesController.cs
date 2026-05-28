using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketMindAPI.Models;

namespace MarketMindAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/favorites
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var favorites = await _db.Favorites.ToListAsync();
        return Ok(favorites);
    }

    // POST /api/favorites
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Favorite favorite)
    {
        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();
        return Ok(favorite);
    }

    // DELETE /api/favorites/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var favorite = await _db.Favorites.FindAsync(id);
        if (favorite == null) return NotFound();
        
        _db.Favorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return Ok("Silindi!");
    }
}