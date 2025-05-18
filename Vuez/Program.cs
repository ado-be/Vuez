using Microsoft.EntityFrameworkCore;
using vuez;

var builder = WebApplication.CreateBuilder(args);

// Na��tajte pripojovac� re�azec z appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Konfigur�cia DbContext s pou�it�m pripojovacieho re�azca
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
