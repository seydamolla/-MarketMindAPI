using System.Text.Json;

namespace MarketMindAPI.Services;

public class StockQuote
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public long Volume { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? PeRatio { get; set; }
    public decimal? Week52High { get; set; }
    public decimal? Week52Low { get; set; }
    public decimal? DividendYield { get; set; }
}

public class HistoryPoint
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
}

public class StockService
{
    private readonly HttpClient _http;

    public StockService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<StockQuote?> GetQuoteAsync(string ticker)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(ticker)}?interval=1d&range=1d";
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var result = root.GetProperty("chart").GetProperty("result")[0];
            var meta = result.GetProperty("meta");

            var price = meta.TryGetProperty("regularMarketPrice", out var p) ? p.GetDecimal() : 0m;
            var prevClose = meta.TryGetProperty("chartPreviousClose", out var pc) ? pc.GetDecimal() : 0m;
            var changePercent = prevClose != 0 ? Math.Round((price - prevClose) / prevClose * 100, 2) : 0m;

            return new StockQuote
            {
                Symbol = ticker,
                Name = meta.TryGetProperty("longName", out var ln) ? ln.GetString() ?? ticker : ticker,
                Price = price,
                ChangePercent = changePercent,
                Open = meta.TryGetProperty("regularMarketOpen", out var o) ? o.GetDecimal() : 0m,
                High = meta.TryGetProperty("regularMarketDayHigh", out var h) ? h.GetDecimal() : 0m,
                Low = meta.TryGetProperty("regularMarketDayLow", out var l) ? l.GetDecimal() : 0m,
                Volume = meta.TryGetProperty("regularMarketVolume", out var v) ? v.GetInt64() : 0L,
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<HistoryPoint>> GetHistoryAsync(string ticker, string period = "1mo")
    {
        try
        {
            var (range, interval) = period switch
            {
                "7d"  => ("7d",  "1d"),
                "1mo" => ("1mo", "1d"),
                "3mo" => ("3mo", "1d"),
                "6mo" => ("6mo", "1wk"),
                "1y"  => ("1y",  "1wk"),
                _     => ("1mo", "1d")
            };

            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(ticker)}?interval={interval}&range={range}";
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return [];

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var result = doc.RootElement.GetProperty("chart").GetProperty("result")[0];
            var timestamps = result.GetProperty("timestamp").EnumerateArray().ToList();
            var indicators = result.GetProperty("indicators").GetProperty("quote")[0];

            var opens   = indicators.GetProperty("open").EnumerateArray().ToList();
            var highs   = indicators.GetProperty("high").EnumerateArray().ToList();
            var lows    = indicators.GetProperty("low").EnumerateArray().ToList();
            var closes  = indicators.GetProperty("close").EnumerateArray().ToList();
            var volumes = indicators.GetProperty("volume").EnumerateArray().ToList();

            var history = new List<HistoryPoint>();
            for (int i = 0; i < timestamps.Count; i++)
            {
                if (closes[i].ValueKind == JsonValueKind.Null) continue;
                history.Add(new HistoryPoint
                {
                    Date   = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).UtcDateTime,
                    Open   = opens[i].ValueKind   != JsonValueKind.Null ? opens[i].GetDecimal()   : 0m,
                    High   = highs[i].ValueKind   != JsonValueKind.Null ? highs[i].GetDecimal()   : 0m,
                    Low    = lows[i].ValueKind    != JsonValueKind.Null ? lows[i].GetDecimal()    : 0m,
                    Close  = closes[i].GetDecimal(),
                    Volume = volumes[i].ValueKind != JsonValueKind.Null ? volumes[i].GetInt64()   : 0L,
                });
            }
            return history;
        }
        catch
        {
            return [];
        }
    }

    public async Task<StockQuote?> GetBist100Async()
        => await GetQuoteAsync("XU100.IS");
}
