namespace MarketMindAPI.Models;

public class PortfolioItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public int Lots { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
