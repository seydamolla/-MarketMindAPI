var builder = WebApplication.CreateBuilder(args);

// Controller'ları ekle
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "MarketMind API çalışıyor! 📈");

// Controller'ları kullan
app.MapControllers();

app.Run();