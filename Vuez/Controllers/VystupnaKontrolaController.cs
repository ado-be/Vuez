using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vuez.Models;

namespace vuez.Controllers
{
    public class VystupnaKontrolaController : Controller
    {
        private readonly AppDbContext _context;

        public VystupnaKontrolaController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.VystupnaKontrola.ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Create()
        {
            int nextProtokol = GetNextProtocolNumber();

            var model = new VystupnaKontrola
            {
                CisloProtokolu = nextProtokol
            };

            // 👉 Načítanie podpisu aktuálneho používateľa (ak má)
            var user = await _context.Users
                                     .Include(u => u.Details)
                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.PodpisUrl = user?.Details?.SignatureImagePath;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VystupnaKontrola model)
        {
            if (ModelState.IsValid)
            {
                model.CisloProtokolu = GetNextProtocolNumber();
                _context.VystupnaKontrola.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // 👉 Opäť načítať podpis pri chybách
            var user = await _context.Users
                                     .Include(u => u.Details)
                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.PodpisUrl = user?.Details?.SignatureImagePath;

            return View(model);
        }

        private int GetNextProtocolNumber()
        {
            int maxProtokol = _context.VystupnaKontrola.Max(v => (int?)v.CisloProtokolu) ?? 0;
            int nextProtokol = maxProtokol + 1;
            Console.WriteLine($"Max číslo protokolu: {maxProtokol}, Nasledujúce číslo: {nextProtokol}");
            return nextProtokol;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vystupnaKontrolaFromDb = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrolaFromDb == null)
                return NotFound();

            // 👉 Aj v edit načítať podpis, ak potrebné
            var user = await _context.Users
                                     .Include(u => u.Details)
                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.PodpisUrl = user?.Details?.SignatureImagePath;

            return View(vystupnaKontrolaFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VystupnaKontrola obj)
        {
            if (!ModelState.IsValid)
                return View(obj);

            var original = await _context.VystupnaKontrola
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (original == null)
                return NotFound();

            obj.CisloProtokolu = original.CisloProtokolu;

            _context.VystupnaKontrola.Update(obj);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

   

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vystupnaKontrola = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrola == null)
                return NotFound();

            return View(vystupnaKontrola);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vystupnaKontrola = await _context.VystupnaKontrola
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (vystupnaKontrola == null)
                return NotFound();

            return View(vystupnaKontrola);
        }

        // POST: VystupnaKontrola/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vystupnaKontrola = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrola == null)
            {
                TempData["ErrorMessage"] = "Položku sa nepodarilo nájsť, nebola odstránená.";
                return RedirectToAction("Index");
            }

            try
            {
                _context.VystupnaKontrola.Remove(vystupnaKontrola);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Položka bola úspešne odstránená.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Chyba pri odstraňovaní: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
