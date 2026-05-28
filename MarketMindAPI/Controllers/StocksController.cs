using Microsoft.AspNetCore.Mvc;

namespace MarketMindAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    // GET /api/stocks
    [HttpGet]
    public IActionResult GetAll()
    {
        var stocks = new[]
        {
            new { Symbol = "THYAO.IS", Name = "Türk Hava Yolları" },
            new { Symbol = "GARAN.IS", Name = "Garanti Bankası" },
            new { Symbol = "ASELS.IS", Name = "Aselsan" },
            new { Symbol = "BIMAS.IS", Name = "BİM" },
            new { Symbol = "POLTK.IS", Name = "Poltek" }
        };
        return Ok(stocks);
    }

    // GET /api/stocks/THYAO.IS
    [HttpGet("{ticker}")]
    public IActionResult GetByTicker(string ticker)
    {
        return Ok(new
        {
            Symbol = ticker.ToUpper(),
            Message = $"{ticker.ToUpper()} verisi",
            Timestamp = DateTime.Now
        });
    }
}