using Microsoft.AspNetCore.Mvc;

using vuez.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace vuez.Controllers
{
    public class RegisterController : Controller
    {
        private readonly AppDbContext _context;

        public RegisterController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string role)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == username))
            {
                ViewBag.ErrorMessage = "Používateľské meno už existuje.";
                return View("Index");
            }

            var roleEntity = await _context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.ErrorMessage = "Vybraná rola neexistuje.";
                return View("Index");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                PasswordHash = hashedPassword,
                RoleId = roleEntity.Id // ✅ Používame správne prepojenie cez RoleId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Login");
        }

    }
}