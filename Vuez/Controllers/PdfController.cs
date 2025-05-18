using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vuez.Models;
using System.Threading.Tasks;

namespace vuez.Controllers
{
    public class PdfController : Controller
    {
        private readonly AppDbContext _context;

        public PdfController(AppDbContext context)
        {
            _context = context;
        }

        // Zobrazenie zoznamu PDF dokumentov
        public async Task<IActionResult> Index()
        {
            var pdfs = await _context.PdfDocuments.ToListAsync();
            return View(pdfs); // Odovzdá zoznam dokumentov do View
        }

        // Stiahnutie PDF súboru podľa ID
        public async Task<IActionResult> Download(int id)
        {
            var pdf = await _context.PdfDocuments.FindAsync(id);
            if (pdf == null)
            {
                return NotFound(); // Vráti 404, ak dokument neexistuje
            }

            // Vráti PDF súbor ako prílohu
            return File(pdf.FileData, "application/pdf", pdf.Name);
        }

        // Zobrazenie formulára na vytvorenie nového PDF dokumentu
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Vráti formulár na nahranie PDF
        }

        // Spracovanie nahrávania PDF súboru do databázy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Vyberte platný súbor na nahranie.");
                return View();
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var pdfDocument = new PdfDocument
                {
                    Name = file.FileName,
                    FileData = memoryStream.ToArray(),
                    UploadedAt = DateTime.UtcNow
                };

                _context.PdfDocuments.Add(pdfDocument);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Presmerovanie na zoznam dokumentov
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Nastala chyba pri ukladaní: {ex.Message}");
                return View();
            }
        }

        // Odstránenie PDF súboru podľa ID
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var pdf = await _context.PdfDocuments.FindAsync(id);
            if (pdf == null)
            {
                return NotFound(); // 404, ak dokument neexistuje
            }

            _context.PdfDocuments.Remove(pdf);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Presmerovanie na zoznam dokumentov
        }
    }
}
