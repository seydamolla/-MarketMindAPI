using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MarketMindAPI.Models;
using MarketMindAPI.Services;

namespace MarketMindAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly StockService _stocks;

    public PortfolioController(AppDbContext db, StockService stocks)
    {
        _db = db;
        _stocks = stocks;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Kullanıcının portföyünü listele (canlı fiyat + kar/zarar dahil)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Portfolio
            .Where(p => p.UserId == UserId)
            .OrderByDescending(p => p.AddedAt)
            .ToListAsync();

        var result = new List<object>();
        decimal totalCost = 0, totalValue = 0;

        foreach (var item in items)
        {
            var quote = await _stocks.GetQuoteAsync(item.Ticker);
            decimal currentPrice = quote?.Price ?? 0m;
            decimal cost = item.BuyPrice * item.Lots;
            decimal value = currentPrice * item.Lots;
            decimal pnl = value - cost;
            decimal pnlPct = cost != 0 ? Math.Round(pnl / cost * 100, 2) : 0m;

            totalCost += cost;
            totalValue += value;

            result.Add(new
            {
                item.Id,
                item.Ticker,
                item.Name,
                item.BuyPrice,
                item.Lots,
                CurrentPrice = currentPrice,
                Cost = Math.Round(cost, 2),
                Value = Math.Round(value, 2),
                PnL = Math.Round(pnl, 2),
                PnLPercent = pnlPct,
                item.AddedAt
            });
        }

        return Ok(new
        {
            Items = result,
            Summary = new
            {
                TotalCost = Math.Round(totalCost, 2),
                TotalValue = Math.Round(totalValue, 2),
                TotalPnL = Math.Round(totalValue - totalCost, 2),
                TotalPnLPercent = totalCost != 0
                    ? Math.Round((totalValue - totalCost) / totalCost * 100, 2)
                    : 0m
            }
        });
    }

    /// <summary>Portföye hisse ekle</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddPortfolioDto dto)
    {
        if (dto.BuyPrice <= 0) return BadRequest(new { error = "Alış fiyatı 0'dan büyük olmalı." });
        if (dto.Lots <= 0) return BadRequest(new { error = "Lot miktarı 0'dan büyük olmalı." });

        var item = new PortfolioItem
        {
            UserId = UserId,
            Ticker = dto.Ticker.ToUpper(),
            Name = dto.Name ?? dto.Ticker.ToUpper(),
            BuyPrice = dto.BuyPrice,
            Lots = dto.Lots,
            AddedAt = DateTime.UtcNow
        };

        _db.Portfolio.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    /// <summary>Portföyden hisse kaldır</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Portfolio.FindAsync(id);
        if (item == null) return NotFound();
        if (item.UserId != UserId) return Forbid();

        _db.Portfolio.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Silindi." });
    }

    /// <summary>Portföy kalemini güncelle (lot veya alış fiyatı)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePortfolioDto dto)
    {
        var item = await _db.Portfolio.FindAsync(id);
        if (item == null) return NotFound();
        if (item.UserId != UserId) return Forbid();

        if (dto.BuyPrice.HasValue && dto.BuyPrice > 0) item.BuyPrice = dto.BuyPrice.Value;
        if (dto.Lots.HasValue && dto.Lots > 0) item.Lots = dto.Lots.Value;

        await _db.SaveChangesAsync();
        return Ok(item);
    }
}

public record AddPortfolioDto(string Ticker, string? Name, decimal BuyPrice, int Lots);
public record UpdatePortfolioDto(decimal? BuyPrice, int? Lots);
