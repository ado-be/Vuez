using Microsoft.AspNetCore.Mvc;
using vuez;

namespace vuez.Controllers
{
    public class IndicatorsController : Controller // Premenované na IndicatorsController
    {
        private readonly AppDbContext _db;

        public IndicatorsController(AppDbContext db)
        {
            _db = db;
        }

        // Index - zobrazenie zoznamu
        public IActionResult Index()
        {
            List<Indicators> objIndicatorsList = _db.Indicators.ToList();
            return View(objIndicatorsList);
        }

        // Create - zobrazenie formulára na vytvorenie
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Indicators obj)
        {
            if (ModelState.IsValid)
            {
                _db.Indicators.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Indicator created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // Edit - zobrazenie formulára na úpravu
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Indicators? indicatorFromDb = _db.Indicators.Find(id);
            if (indicatorFromDb == null)
            {
                return NotFound();
            }
            return View(indicatorFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Indicators obj)
        {
            if (ModelState.IsValid)
            {
                _db.Indicators.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Indicator updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // Delete - zobrazenie potvrdenia odstránenia
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Indicators? indicatorFromDb = _db.Indicators.Find(id);
            if (indicatorFromDb == null)
            {
                return NotFound();
            }
            return View(indicatorFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Indicators? obj = _db.Indicators.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            _db.Indicators.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Indicator deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
