using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using vuez.Models;
using vuez.Models.ViewModels;

namespace vuez.Controllers
{
    public class MedzioperacnaKontrola : Controller
    {
        private readonly AppDbContext _context;

        public MedzioperacnaKontrola(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Načítame všetky konfiguračné listy z databázy
                var configurationSheets = await _context.ConfigurationSheets
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                // Vypíšeme počet nájdených záznamov pre diagnostiku
                System.Diagnostics.Debug.WriteLine($"Nájdených {configurationSheets.Count} konfiguračných listov");

                // Vrátime view s dátami
                return View(configurationSheets);
            }
            catch (Exception ex)
            {
                // Ak nastane chyba, zalogujeme ju a vrátime prázdny zoznam
                System.Diagnostics.Debug.WriteLine($"Chyba pri načítaní konfiguračných listov: {ex.Message}");
                return View(new List<ConfigurationSheet>());
            }
        }

        public IActionResult Create() => View(new ConfigurationSheetViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigurationSheetViewModel model, string action)
        {
            try
            {
                // Detailné logovanie všetkých prijatých dát
                System.Diagnostics.Debug.WriteLine("---- VŠETKY PRIJATÉ DÁTA ----");
                System.Diagnostics.Debug.WriteLine($"ConfigurationSheet: {model.ConfigurationSheet?.Apvname}, {model.ConfigurationSheet?.Apvnumber}");

                if (model.ProgramItems != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Počet položiek pred filtráciou: {model.ProgramItems.Count}");
                    for (int i = 0; i < model.ProgramItems.Count; i++)
                    {
                        var item = model.ProgramItems[i];
                        System.Diagnostics.Debug.WriteLine($"Položka {i}: {item.ItemCode ?? "(prázdne)"}, {item.ItemName ?? "(prázdne)"}, {item.ItemDescription ?? "(prázdne)"}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ProgramItems je null!");
                    model.ProgramItems = new List<ProgramItem>();
                }

                // Najprv filtrujeme prázdne položky
                model.ProgramItems = model.ProgramItems
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(item.ItemCode) &&
                        !string.IsNullOrWhiteSpace(item.ItemName))
                    .ToList();

                // Logovanie filtrovaných položiek
                System.Diagnostics.Debug.WriteLine($"Počet položiek po filtrácii: {model.ProgramItems.Count}");
                foreach (var item in model.ProgramItems)
                {
                    System.Diagnostics.Debug.WriteLine($"Platná položka: {item.ItemCode}, {item.ItemName}, {item.ItemDescription}");
                }

                // Odstránenie nepotrebných ModelState kľúčov
                var keysToRemove = ModelState.Keys
                    .Where(key => key.StartsWith("ProgramItems["))
                    .ToList();

                foreach (var key in keysToRemove)
                    ModelState.Remove(key);

                // Pridanie ValidateModelState len pre filtrované položky
                for (int i = 0; i < model.ProgramItems.Count; i++)
                {
                    var item = model.ProgramItems[i];
                    if (string.IsNullOrWhiteSpace(item.ItemCode))
                        ModelState.AddModelError($"ProgramItems[{i}].ItemCode", "Kód položky je povinný.");
                    if (string.IsNullOrWhiteSpace(item.ItemName))
                        ModelState.AddModelError($"ProgramItems[{i}].ItemName", "Názov položky je povinný.");
                }

                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("Model nie je validný:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"- {error.ErrorMessage}");
                    }
                    return View(model);
                }

                // Uloženie hlavného konfiguračného listu
                model.ConfigurationSheet.CreatedDate = DateTime.UtcNow;
                _context.ConfigurationSheets.Add(model.ConfigurationSheet);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Uložený konfiguračný list s ID: {model.ConfigurationSheet.ConfigId}");

                // Ak niet programových položiek, vytvoríme aspoň jednu prázdnu
                if (!model.ProgramItems.Any() && action == "continue")
                {
                    var emptyItem = new ProgramItem
                    {
                        ConfigId = model.ConfigurationSheet.ConfigId,
                        ItemCode = "Položka 1",
                        ItemName = "Nová položka",
                        ItemDescription = "Automaticky vytvorená položka"
                    };
                    _context.ProgramItems.Add(emptyItem);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Vytvorená automatická položka s ID: {emptyItem.ItemId}");
                }
                else
                {
                    // Inak uložíme zadané položky
                    foreach (var item in model.ProgramItems)
                    {
                        item.ConfigId = model.ConfigurationSheet.ConfigId;
                        _context.ProgramItems.Add(item);
                        System.Diagnostics.Debug.WriteLine($"Pridaná položka: {item.ItemCode}, {item.ItemName}");
                    }

                    if (model.ProgramItems.Any())
                        await _context.SaveChangesAsync();
                }

                System.Diagnostics.Debug.WriteLine($"Akcia: {action}, Presmerujeme na: {(action == "save" ? "Create" : "Configlist")}");
                return action == "save"
                    ? RedirectToAction("Create")
                    : RedirectToAction("Configlist", new { configId = model.ConfigurationSheet.ConfigId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CHYBA pri vytváraní: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                ModelState.AddModelError("", $"Nastala neočakávaná chyba: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Configlist(int configId, int? index)
        {
            var sheet = await _context.ConfigurationSheets.FindAsync(configId);
            if (sheet == null)
                return NotFound();

            var items = await _context.ProgramItems
                .Where(p => p.ConfigId == configId)
                .OrderBy(p => p.ItemId)
                .Include(p => p.Config)
                .ToListAsync();

            if (index == null)
            {
                // Vytvorím ProgramItemConfigViewModel namiesto ConfigurationConfigListViewModel
                var item = items.FirstOrDefault();
                var detail = item != null
                    ? await _context.ProgramItemDetails
                        // Odstránime Include(d => d.ProgramRelease)
                        .FirstOrDefaultAsync(d => d.ItemId == item.ItemId)
                    : null;

                if (detail == null && item != null)
                {
                    detail = new ProgramItemDetail
                    {
                        ItemId = item.ItemId,
                        Ppname = item.ItemName,
                        Ppnumber = item.ItemCode,
                        RelatedDocumentation = sheet.RelatedDocumentation,
                        Connections = sheet.RelatedHwsw
                    };
                }

                // Odstránime referenciu na ProgramRelease
                // var release = detail?.ProgramRelease ?? new ProgramRelease { Detail = detail };

                var summaryModel = new ProgramItemConfigViewModel
                {
                    Item = item,
                    Detail = detail,
                    // Odstránime Release = release,

                    // Pridám zoznam položiek a konfiguračný list pre prehľad
                    AllItems = items,
                    ConfigurationSheet = sheet
                };

                return View("Configlist", summaryModel);
            }

            if (!items.Any() || index >= items.Count)
                return RedirectToAction("ExamRecord", new { configId });

            var selectedItem = await _context.ProgramItems
                .Include(p => p.Config)
                .FirstOrDefaultAsync(p => p.ItemId == items[index.Value].ItemId);

            if (selectedItem == null)
                return NotFound();

            var selectedDetail = await _context.ProgramItemDetails
                // Odstránime Include(d => d.ProgramRelease)
                .FirstOrDefaultAsync(d => d.ItemId == selectedItem.ItemId);

            if (selectedDetail == null)
            {
                selectedDetail = new ProgramItemDetail
                {
                    ItemId = selectedItem.ItemId,
                    Ppname = selectedItem.ItemName,
                    Ppnumber = selectedItem.ItemCode,
                    RelatedDocumentation = sheet.RelatedDocumentation,
                    Connections = sheet.RelatedHwsw
                };
            }

            // Odstránime referenciu na ProgramRelease
            // var selectedRelease = selectedDetail.ProgramRelease ?? new ProgramRelease { Detail = selectedDetail };

            var model = new ProgramItemConfigViewModel
            {
                Item = selectedItem,
                Detail = selectedDetail,
                // Odstránime Release = selectedRelease,

                // Pridám zoznam položiek a konfiguračný list pre konzistenciu
                AllItems = items,
                ConfigurationSheet = sheet
            };

            return View("ProgramItemForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgramItem(ProgramItemConfigViewModel model, int index)
        {
            if (!ModelState.IsValid)
                return View("ProgramItemForm", model);

            try
            {
                // Logovanie pre diagnostiku
                System.Diagnostics.Debug.WriteLine($"Saving Item: {model.Item.ItemId}, {model.Item.ItemCode}, {model.Item.ItemName}");
                System.Diagnostics.Debug.WriteLine($"Detail: {model.Detail?.DetailId}, {model.Detail?.Ppname}, {model.Detail?.Ppnumber}");

                // 1. Aktualizácia programovej položky
                var existingItem = await _context.ProgramItems
                    .FirstOrDefaultAsync(p => p.ItemId == model.Item.ItemId);

                if (existingItem != null)
                {
                    // Aktualizujeme existujúcu položku
                    existingItem.ItemCode = model.Item.ItemCode;
                    existingItem.ItemName = model.Item.ItemName;
                    existingItem.ItemDescription = model.Item.ItemDescription;

                    // Explicitne označíme ako zmenený
                    _context.Entry(existingItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Updated Item: {existingItem.ItemId}");
                }
                else
                {
                    // Tento prípad by nemal nastať, ale pre istotu
                    System.Diagnostics.Debug.WriteLine("No existing item found to update!");
                    return NotFound("Položka nebola nájdená");
                }

                // 2. Aktualizácia alebo vytvorenie programového detailu
                if (model.Detail != null)
                {
                    // Vyhľadanie detailu podľa ItemId (nie DetailId)
                    var existingDetail = await _context.ProgramItemDetails
                        .FirstOrDefaultAsync(d => d.ItemId == model.Item.ItemId);

                    if (existingDetail == null)
                    {
                        // Vytvorenie nového detailu
                        var newDetail = new ProgramItemDetail
                        {
                            ItemId = model.Item.ItemId,
                            Ppname = model.Detail.Ppname,
                            Ppnumber = model.Detail.Ppnumber,
                            DevelopmentTools = model.Detail.DevelopmentTools,
                            DevelopmentPc = model.Detail.DevelopmentPc,
                            Connections = model.Detail.Connections,
                            RelatedDocumentation = model.Detail.RelatedDocumentation
                            // Pridajte ďalšie polia podľa potreby
                        };

                        _context.ProgramItemDetails.Add(newDetail);
                        System.Diagnostics.Debug.WriteLine($"Added new detail for ItemId: {model.Item.ItemId}");
                    }
                    else
                    {
                        // Aktualizácia existujúceho detailu
                        existingDetail.Ppname = model.Detail.Ppname;
                        existingDetail.Ppnumber = model.Detail.Ppnumber;
                        existingDetail.DevelopmentTools = model.Detail.DevelopmentTools;
                        existingDetail.DevelopmentPc = model.Detail.DevelopmentPc;
                        existingDetail.Connections = model.Detail.Connections;
                        existingDetail.RelatedDocumentation = model.Detail.RelatedDocumentation;

                        // Explicitne označíme ako zmenený
                        _context.Entry(existingDetail).State = EntityState.Modified;
                        System.Diagnostics.Debug.WriteLine($"Updated existing detail ID: {existingDetail.DetailId}");
                    }

                    // Uloženie zmien detailu
                    await _context.SaveChangesAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Model.Detail is null!");
                }

                // Pokračujeme na ďalšiu položku alebo na záznam o kontrole
                var itemList = await _context.ProgramItems
                    .Where(p => p.ConfigId == model.Item.ConfigId)
                    .OrderBy(p => p.ItemId)
                    .ToListAsync();

                return index + 1 < itemList.Count
                    ? RedirectToAction("Configlist", new { configId = model.Item.ConfigId, index = index + 1 })
                    : RedirectToAction("ExamRecord", new { configId = model.Item.ConfigId });
            }
            catch (Exception ex)
            {
                // Zachytenie a logovanie chyby
                System.Diagnostics.Debug.WriteLine($"Error in SaveProgramItem: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                // Vrátime chybu používateľovi
                ModelState.AddModelError("", $"Chyba pri ukladaní: {ex.Message}");
                return View("ProgramItemForm", model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int configId)
        {
            var config = await _context.ConfigurationSheets
                .Include(c => c.ProgramItems)
                .FirstOrDefaultAsync(c => c.ConfigId == configId);

            if (config == null)
            {
                return NotFound();
            }

            return View(config);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int configId)
        {
            try
            {
                var config = await _context.ConfigurationSheets
                    .Include(c => c.ProgramItems)
                    .FirstOrDefaultAsync(c => c.ConfigId == configId);

                if (config == null)
                {
                    return NotFound();
                }

                // Najprv odstránime všetky detaily programových položiek (ak existujú)
                var detailIds = await _context.ProgramItemDetails
                    .Where(d => config.ProgramItems.Select(p => p.ItemId).Contains(d.ItemId))
                    .Select(d => d.DetailId)
                    .ToListAsync();

                if (detailIds.Any())
                {
                    var details = await _context.ProgramItemDetails
                        .Where(d => detailIds.Contains(d.DetailId))
                        .ToListAsync();

                    _context.ProgramItemDetails.RemoveRange(details);
                    await _context.SaveChangesAsync();
                }

                // Potom odstránime všetky programové položky
                _context.ProgramItems.RemoveRange(config.ProgramItems);

                // Nakoniec odstránime samotný konfiguračný list
                _context.ConfigurationSheets.Remove(config);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Konfiguračný list bol úspešne vymazaný.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Nastala chyba pri vymazávaní: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult ExamRecord() => View();
    }
}
