using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vuez.Models;
using vuez.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace vuez.Controllers
{
    [Authorize(Roles = "Admin")] // ✅ Povolené len pre admina
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService; // ✅ Pridanie EmailService

        public AdminController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService; // ✅ Inicializácia EmailService
        }

        // ✅ Zobrazenie zoznamu používateľov
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // ✅ Zobrazenie formulára na pridanie nového používateľa
        public IActionResult CreateUser()
        {
            return View();
        }


        [HttpPost]
        public IActionResult DeleteUser(Guid id) 
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("ManageUsers");
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string email, string role, string name,string surname)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == username))
            {
                ViewBag.ErrorMessage = "Používateľ už existuje.";
                return View();
            }

            // ✅ Vygenerovanie náhodného hesla
            string generatedPassword = GenerateRandomPassword();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

            // ✅ Nájdeme ID roly
            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.ErrorMessage = "Rola neexistuje.";
                return View();
            }

            // ✅ Vytvorenie nového používateľa
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                PasswordHash = hashedPassword,
                RoleId = roleEntity.Id
            };

            // Vytvorenie a priradenie UserDetails
            var userDetails = new UserDetails
            {
                UserId = newUser.Id,
                Name = name,
                Surname = surname,
                Email = email
            };
            _context.Users.Add(newUser);
            _context.UserDetails.Add(userDetails);
            await _context.SaveChangesAsync();

            // ✅ Pošleme e-mail s heslom
            string subject = "Vaše nové heslo";
            string message = $"Dobrý deň {username},\n\nVaše vygenerované heslo je: {generatedPassword}\n\nPo prihlásení si ho zmeňte.";

            await _emailService.SendEmailAsync(email, subject, message);

            // ✅ Po pridaní zobrazíme heslo adminovi
            ViewBag.GeneratedPassword = generatedPassword;
            ViewBag.SuccessMessage = $"Používateľ {username} bol úspešne vytvorený.";

            return View();
        }

        // ✅ Generovanie bezpečného hesla
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
