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

        // GET: VystupnaKontrola
        public async Task<IActionResult> Index()
        {
            var data = await _context.VystupnaKontrola.ToListAsync();
            return View(data);
        }

        // GET: VystupnaKontrola/Create
        public IActionResult Create()
        {
            int nextProtokol = GetNextProtocolNumber();
            var model = new VystupnaKontrola
            {
                CisloProtokolu = nextProtokol
            };

            return View(model);
        }

        // POST: VystupnaKontrola/Create
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

            return View(model);
        }

        private int GetNextProtocolNumber()
        {
            int maxProtokol = _context.VystupnaKontrola.Max(v => (int?)v.CisloProtokolu) ?? 0;
            int nextProtokol = maxProtokol + 1;

            Console.WriteLine($"Max číslo protokolu: {maxProtokol}, Nasledujúce číslo: {nextProtokol}");

            return nextProtokol;
        }

        // GET: VystupnaKontrola/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vystupnaKontrolaFromDb = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrolaFromDb == null)
                return NotFound();

            return View(vystupnaKontrolaFromDb);
        }

        // POST: VystupnaKontrola/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VystupnaKontrola obj)
        {
            if (!ModelState.IsValid)
                return View(obj);

            // 🔧 Zabezpečiť, že CisloProtokolu sa zachová, ak sa nestratí z formulára
            var original = await _context.VystupnaKontrola
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.Id == obj.Id);

            if (original == null)
                return NotFound();

            // Zabezpečí, že CisloProtokolu nebude NULL
            obj.CisloProtokolu = original.CisloProtokolu;

            _context.VystupnaKontrola.Update(obj);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var vystupnaKontrola = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrola == null)
                return NotFound();

            return View(vystupnaKontrola);
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

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vystupnaKontrola = await _context.VystupnaKontrola.FindAsync(id);
            if (vystupnaKontrola == null)
                return NotFound();

            _context.VystupnaKontrola.Remove(vystupnaKontrola);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
