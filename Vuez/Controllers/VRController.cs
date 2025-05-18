using Microsoft.AspNetCore.Mvc;

namespace vuez.Controllers
{
    public class VRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
