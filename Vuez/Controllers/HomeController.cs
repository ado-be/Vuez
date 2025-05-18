// HomeController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Layout = null (alebo string.Empty) - zobrazi� domovsk� str�nku bez login layoutu
        return View("Index");
    }
}