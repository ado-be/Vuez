using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vuez.Models;
using vuez.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;

namespace vuez.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string role)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == username))
            {
                ViewBag.Error = "Používateľské meno už existuje.";
                return View();
            }

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role);
            if (roleEntity == null)
            {
                ViewBag.Error = "Vybraná rola neexistuje.";
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = username,
                PasswordHash = hashedPassword,
                RoleId = roleEntity.Id
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Používateľ neexistuje.";
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Staré heslo je nesprávne.";
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Heslo bolo úspešne zmenené.";
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var user = await _context.Users
                .Include(u => u.Details)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var viewModel = new UserProfileViewModel
            {
                Name = user.Details?.Name ?? "",
                Surname = user.Details?.Surname ?? "",
                Email = user.Details?.Email ?? "",
                SignatureImagePath = user.Details?.SignatureImagePath
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Formulár nebol správne vyplnený.");
                return View("Index", model);
            }

            var username = User.Identity.Name;
            var user = await _context.Users
                .Include(u => u.Details)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (user.Details == null)
            {
                user.Details = new UserDetails
                {
                    UserId = user.Id,
                    Id = Guid.NewGuid()
                };
                _context.UserDetails.Add(user.Details);
            }

            user.Details.Name = model.Name;
            user.Details.Surname = model.Surname;
            user.Details.Email = model.Email;
            user.Details.LastUpdated = DateTime.UtcNow;

            if (model.SignatureFile != null && model.SignatureFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.SignatureFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SignatureFile.CopyToAsync(stream);
                    }

                    user.Details.SignatureImagePath = $"/signatures/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Nepodarilo sa uložiť obrázok: " + ex.Message);
                    return View("Index", model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profil bol úspešne aktualizovaný.";
            return RedirectToAction("Index");
        }
    }
}
