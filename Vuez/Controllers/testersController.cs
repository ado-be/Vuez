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
    public class testersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
