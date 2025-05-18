using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using vuez.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace vuez.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

     
        [HttpGet]
        public async Task<IActionResult> Index(string? returnUrl = null)
        {
          
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authenticate(string username, string password, string? returnUrl = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Zadajte používateľské meno a heslo.";
                    return View("Index");
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserName == username);

                
                if (user == null || !BCrypt.Net.BCrypt.Verify(password.Trim(), user.PasswordHash))
                {
                    TempData["Error"] = "Nesprávne meno alebo heslo.";  
                    return RedirectToAction("Index", new { returnUrl }); 
                }

               
                if (user.Role == null)
                {
                    TempData["Error"] = "Používateľ nemá priradenú rolu.";  
                    return RedirectToAction("Index", new { returnUrl });     
                }


                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"‼️ Chyba pri prihlásení: {ex.Message}");
                ViewBag.Error = "Nastala chyba pri prihlásení: " + ex.Message;
                return View("Index");
            }
        }

      
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

              
                Response.Cookies.Delete("UserLoginCookie");

                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba pri odhlásení: {ex.Message}");
                return StatusCode(500, "Nastala chyba pri odhlásení: " + ex.Message);
            }
        }
    }
}
