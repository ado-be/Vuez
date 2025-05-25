using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using vuez.Models;
using System;
using System.Linq;

namespace vuez.Controllers
{
    public class VstupnaKontrolaController : Controller
    {
        private readonly AppDbContext _context;

        public VstupnaKontrolaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            int nextProtokol = GetNextProtocolNumber();

            var model = new VstupnaKontrola
            {
                CisloProtokolu = nextProtokol
            };

            // Získanie podpisu prihláseného používateľa
            var user = await _context.Users
                .Include(u => u.Details)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ViewBag.PodpisUrl = user?.Details?.SignatureImagePath;

            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VstupnaKontrola model)
        {
            if (ModelState.IsValid)
            {
                model.CisloProtokolu = GetNextProtocolNumber();
                Console.WriteLine($"Ukladám protokol s číslom: {model.CisloProtokolu}");
                _context.VstupnaKontrola.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Opäť získame podpis aj pri chybách
            var user = await _context.Users
                .Include(u => u.Details)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            ViewBag.PodpisUrl = user?.Details?.SignatureImagePath;

            return View(model);
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var vstupneKontroly = from v in _context.VstupnaKontrola
                                  select v;

            if (!string.IsNullOrEmpty(searchString))
            {
                vstupneKontroly = vstupneKontroly.Where(v =>
                    v.NazovVyrobku.Contains(searchString) ||
                    v.Dodavatel.Contains(searchString) ||
                    v.ZakazkoveCislo.Contains(searchString));
            }

            var data = await vstupneKontroly.ToListAsync();

            var now = DateTime.Now;
            bool hasOverdue = data.Any(c => c.Datum.HasValue && (now - c.Datum.Value).TotalDays > 30);
            bool hasUnsigned = data.Any(c => string.IsNullOrEmpty(c.PodpisManagerUrl) || string.IsNullOrEmpty(c.PodpisTechnikUrl));
            if (hasOverdue || hasUnsigned)
            {
                string message = "<strong>Upozornenie:</strong> ";
                if (hasOverdue) message += "Aspoň jedna vstupná kontrola je staršia ako 30 dní. ";
                if (hasUnsigned) message += "Aspoň jedna vstupná kontrola nemá potrebné podpisy.";
                TempData["OverdueWarning"] = message;

            }

            ViewData["CurrentFilter"] = searchString;

            return View(data);
        }



        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vstupnaKontrola = await _context.VstupnaKontrola.FindAsync(id);
            if (vstupnaKontrola == null)
                return NotFound();

            return View(vstupnaKontrola);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var kontrola = await _context.VstupnaKontrola.FindAsync(id);
            if (kontrola == null)
                return NotFound();

            return View(kontrola);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VstupnaKontrola obj)
        {
            if (ModelState.IsValid)
            {
                _context.VstupnaKontrola.Update(obj);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var kontrola = await _context.VstupnaKontrola.FindAsync(id);
            if (kontrola == null)
                return NotFound();

            return View(kontrola);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kontrola = await _context.VstupnaKontrola.FindAsync(id);
            if (kontrola == null)
                return NotFound();

            _context.VstupnaKontrola.Remove(kontrola);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private int GetNextProtocolNumber()
        {
            int maxProtokol = _context.VstupnaKontrola.Max(v => (int?)v.CisloProtokolu) ?? 0;
            int nextProtokol = maxProtokol + 1;
            Console.WriteLine($"Max číslo protokolu: {maxProtokol}, Nasledujúce číslo: {nextProtokol}");
            return nextProtokol;
        }
    }
}
