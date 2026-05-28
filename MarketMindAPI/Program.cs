using Microsoft.EntityFrameworkCore;
using MarketMindAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Controller'ları ekle
builder.Services.AddControllers();

// Veritabanı ekle (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=marketmind.db"));

var app = builder.Build();

// Veritabanını otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "MarketMind API çalışıyor! 📈");

app.MapControllers();

app.Run();