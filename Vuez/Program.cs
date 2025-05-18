using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using vuez;
using vuez.Models;
using vuez.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ? Nastavenie Identity a DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? Pridanie autentifikácie pomocou cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
   {
       options.LoginPath = "/Login/Index"; // ✅ presmerovanie na LoginController
   
       options.Cookie.Name = "UserLoginCookie";
   });


builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// ? **Inicializácia databázy a vytvorenie admina pri štarte aplikácie**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Automaticky aplikuje migrácie

        // ? Skontroluj, či existujú roly, ak nie, vytvor ich
        if (!context.Roles.Any())
        {
            var adminRole = new Role { Id = Guid.NewGuid(), RoleName = "Admin" };
            var userRole = new Role { Id = Guid.NewGuid(), RoleName = "User" };

            context.Roles.AddRange(adminRole, userRole);
            context.SaveChanges();
            Console.WriteLine("? Roly boli vytvorené.");
        }

        // ? Skúsime pridať admina, ak neexistuje
        var adminRoleId = context.Roles.FirstOrDefault(r => r.RoleName == "Admin")?.Id;
        var userExists = context.Users.Any(u => u.UserName == "admin");

        if (!userExists && adminRoleId != null)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                PasswordHash = hashedPassword,
                RoleId = adminRoleId.Value // ? Teraz používame existujúce Admin RoleId
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("? Predvolený admin bol vytvorený!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Chyba pri inicializácii databázy: {ex.Message}");
    }
}

// ? Middleware pre HTTPS, statické súbory a routovanie
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ? Nastavenie predvoleného routovania
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"); // Predvolený kontroler a akcia

app.Run();
