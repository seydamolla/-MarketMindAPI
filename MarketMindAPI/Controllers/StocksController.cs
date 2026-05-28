using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketMindAPI.Services;

namespace MarketMindAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly StockService _stocks;

    public StocksController(StockService stocks)
    {
        _stocks = stocks;
    }

    /// <summary>BİST 100 endeks özeti</summary>
    [HttpGet("bist100")]
    public async Task<IActionResult> GetBist100()
    {
        var data = await _stocks.GetBist100Async();
        if (data == null) return NotFound(new { error = "BİST100 verisi alınamadı." });
        return Ok(data);
    }

    /// <summary>Hisse güncel fiyat ve temel verileri</summary>
    [HttpGet("{ticker}")]
    public async Task<IActionResult> GetQuote(string ticker)
    {
        var data = await _stocks.GetQuoteAsync(ticker.ToUpper());
        if (data == null) return NotFound(new { error = $"{ticker} bulunamadı." });
        return Ok(data);
    }

    /// <summary>Hisse fiyat geçmişi (OHLCV)</summary>
    [HttpGet("{ticker}/history")]
    public async Task<IActionResult> GetHistory(string ticker, [FromQuery] string period = "1mo")
    {
        var validPeriods = new[] { "7d", "1mo", "3mo", "6mo", "1y" };
        if (!validPeriods.Contains(period))
            return BadRequest(new { error = "Geçerli period: 7d, 1mo, 3mo, 6mo, 1y" });

        var history = await _stocks.GetHistoryAsync(ticker.ToUpper(), period);
        if (history.Count == 0) return NotFound(new { error = "Geçmiş veri bulunamadı." });
        return Ok(history);
    }
}
