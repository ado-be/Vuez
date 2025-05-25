using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using vuez.Models;
using vuez.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vuez.Controllers
{
    public class MedzioperacnaKontrola : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MedzioperacnaKontrola(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<Guid?> GetCurrentUserId()
        {
            try
            {
                var currentUserName = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUserName))
                {
                    System.Diagnostics.Debug.WriteLine("❌ User.Identity.Name je prázdny");
                    return null;
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == currentUserName);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Používateľ {currentUserName} nebol nájdený v databáze");
                    return null;
                }

                // Jednoducho skúsiť parsovať user.Id bez type checking
                try
                {
                    var userIdString = user.Id.ToString();
                    if (Guid.TryParse(userIdString, out Guid userId))
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 GetCurrentUserId: {currentUserName} -> UserId: {userId}");
                        return userId;
                    }
                }
                catch (Exception parseEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Chyba pri parsovaní UserId: {parseEx.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"❌ Nemôžem konvertovať User.Id na Guid");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri získavaní UserId: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var configurationSheets = from c in _context.ConfigurationSheets
                                          select c;

                if (!string.IsNullOrEmpty(searchString))
                {
                    configurationSheets = configurationSheets.Where(c =>
                        c.Apvname.Contains(searchString) ||
                        c.Apvnumber.Contains(searchString) ||
                        c.OrderNumber.Contains(searchString) ||
                        c.Processor.Contains(searchString));
                }

                var resultList = await configurationSheets
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                // Skontrolujeme, či je aspoň jeden protokol starší ako 30 dní
                var now = DateTime.Now;
                bool hasOverdue = resultList.Any(c => c.CreatedDate.HasValue && (now - c.CreatedDate.Value).TotalDays > 30);

               
           
                if (hasOverdue )
                {
                    string message = "<strong>Upozornenie:</strong> ";
                    if (hasOverdue) message += "Aspoň jedna medzioperačná kontrola je staršia ako 30 dní. ";
                   
                    TempData["OverdueWarning"] = message;

                }

                ViewData["CurrentFilter"] = searchString;

              

                return View(resultList);
            }
            catch (Exception ex)
            {
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

            if (!items.Any())
                return RedirectToAction("ExamRecord", new { configId });

            var item = index.HasValue ? items.ElementAtOrDefault(index.Value) : items.FirstOrDefault();
            if (item == null)
                return RedirectToAction("ExamRecord", new { configId });

            // Načítanie existujúceho detailu
            var detail = await _context.ProgramItemDetails
                .FirstOrDefaultAsync(d => d.ItemId == item.ItemId);

            // OPRAVENÉ: Načítanie súvisiacich záznamov S používateľmi pre podpisy
            ProgramReview programReview = null;
            ProgramVerification programVerification = null;
            ProgramRelease programRelease = null;

            if (detail != null)
            {
                // PRIDANÉ: Include používateľov pre načítanie podpisov
                programReview = await _context.ProgramReviews
                    .Include(r => r.ReviewerUser)           // Navigation property pre používateľa
                    .ThenInclude(u => u.Details)            // Podpis používateľa z UserDetail
                    .FirstOrDefaultAsync(r => r.DetailId == detail.DetailId);

                programVerification = await _context.ProgramVerifications
                    .Include(v => v.VerifierUser)           // Navigation property pre používateľa  
                    .ThenInclude(u => u.Details)            // Podpis používateľa z UserDetail
                    .FirstOrDefaultAsync(v => v.DetailId == detail.DetailId);

                programRelease = await _context.ProgramReleases
                    .Include(r => r.ReleasedByUser)         // Navigation property pre používateľa
                    .ThenInclude(u => u.Details)            // Podpis používateľa z UserDetail
                    .FirstOrDefaultAsync(r => r.DetailId == detail.DetailId);

                // PRIDANÉ: Nastavenie podpisových URL do objektov pre view
                if (programReview?.ReviewerUser?.Details?.SignatureImagePath != null)
                {
                    // Ak máte property ReviewerSignature v ProgramReview modeli, nastavte ju:
                    programReview.ReviewerSignature = programReview.ReviewerUser.Details.SignatureImagePath;
                }

                if (programVerification?.VerifierUser?.Details?.SignatureImagePath != null)
                {
                    // Ak máte property ReviewerSignature v ProgramVerification modeli, nastavte ju:
                    programVerification.ReviewerSignature = programVerification.VerifierUser.Details.SignatureImagePath;
                }

                if (programRelease?.ReleasedByUser?.Details?.SignatureImagePath != null)
                {
                    // Ak máte property ReleaseSignature v ProgramRelease modeli, nastavte ju:
                    programRelease.ReleaseSignature = programRelease.ReleasedByUser.Details.SignatureImagePath;
                }
            }

            // Príprava detailu (bez ukladania!)
            if (detail == null)
            {
                detail = new ProgramItemDetail
                {
                    ItemId = item.ItemId,
                    Ppname = item.ItemName,
                    Ppnumber = item.ItemCode,
                    RelatedDocumentation = sheet.RelatedDocumentation,
                    Connections = sheet.RelatedHwsw,
                    ModifiedBy = sheet.Processor
                };

                System.Diagnostics.Debug.WriteLine($"📝 Pripravený nový detail pre ItemId {item.ItemId} (bez uloženia)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"📥 Načítaný existujúci detail pre ItemId {item.ItemId}");
            }

            // Nastavenie podpisu používateľa
            await SetUserSignature();

            var model = new ProgramItemConfigViewModel
            {
                Item = item,
                Detail = detail,
                AllItems = items,
                ConfigurationSheet = sheet,
                ProgramReview = programReview,
                ProgramVerification = programVerification,
                ProgramRelease = programRelease
            };

            return index == null ? View("Configlist", model) : View("ProgramItemForm", model);
        }

        // PRIDANÁ NOVÁ METÓDA PRE NASTAVENIE PODPISU (rovnaký systém ako vo VstupnaKontrolaController)
        private async Task SetUserSignature()
        {
            try
            {
                var currentUser = User.Identity.Name;
                System.Diagnostics.Debug.WriteLine($"🔍 Current user: {currentUser}");

                // Získanie podpisu z UserDetail tabuľky
                var user = await _context.Users
                    .Include(u => u.Details)
                    .FirstOrDefaultAsync(u => u.UserName == currentUser);

                if (user?.Details?.SignatureImagePath != null)
                {
                    ViewBag.UserSignatureUrl = user.Details.SignatureImagePath;
                    System.Diagnostics.Debug.WriteLine($"🔍 User signature path from UserDetail: {user.Details.SignatureImagePath}");
                }
                else
                {
                    ViewBag.UserSignatureUrl = "/images/default-signature.png";
                    System.Diagnostics.Debug.WriteLine($"🔍 No signature found, using default");
                }

                System.Diagnostics.Debug.WriteLine($"🔍 Final ViewBag.UserSignatureUrl: {ViewBag.UserSignatureUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri nastavovaní podpisu: {ex.Message}");
                ViewBag.UserSignatureUrl = "/images/default-signature.png";
            }
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFormData()
        {
            try
            {
                // Získanie aktuálneho používateľa a jeho ID
                var currentUser = User.Identity.Name;
                var currentUserId = await GetCurrentUserId();
                System.Diagnostics.Debug.WriteLine($"📄 SaveFormData: Používateľ {currentUser} (ID: {currentUserId}) ukladá formulář");

                // Získání hodnot přímo z formuláře
                int configId = 0;
                int.TryParse(Request.Form["configId"], out configId);

                int itemId = 0;
                int.TryParse(Request.Form["itemId"], out itemId);

                int detailId = 0;
                int.TryParse(Request.Form["detailId"], out detailId);

                string itemCode = Request.Form["itemCode"];
                string itemName = Request.Form["itemName"];
                string itemDescription = Request.Form["itemDescription"];

                string ppname = Request.Form["ppname"];
                string ppnumber = Request.Form["ppnumber"];
                string modifiedBy = Request.Form["modifiedBy"];
                string initialVersionNumber = Request.Form["initialVersionNumber"];
                string developmentTools = Request.Form["developmentTools"];
                string developmentPc = Request.Form["developmentPc"];
                string connections = Request.Form["connections"];
                string relatedDocumentation = Request.Form["relatedDocumentation"];
                string notes = Request.Form["notes"];

                // Nové údaje pro preskúmanie (ProgramReview)
                string reviewForm = Request.Form["reviewForm"];
                string reviewResult = Request.Form["reviewResult"];
                string reviewer = Request.Form["reviewer"];
                string reviewDate = Request.Form["reviewDate"];
                // ZMENENÉ: Kontrola či používateľ podpísal (namiesto URL kontrolujeme či má podpis v DB)
                bool reviewerSigned = !string.IsNullOrEmpty(Request.Form["reviewerSigned"]) && Request.Form["reviewerSigned"] == "true";

                // Nové údaje pro overenie (ProgramVerification)
                string verificationForm = Request.Form["verificationForm"];
                string verificationResult = Request.Form["verificationResult"];
                string verifier = Request.Form["verifier"];
                string verificationDate = Request.Form["verificationDate"];
                bool verifierSigned = !string.IsNullOrEmpty(Request.Form["verifierSigned"]) && Request.Form["verifierSigned"] == "true";

                // Nové údaje pro uvoľnenie (ProgramRelease)
                string releasedBy = Request.Form["releasedBy"];
                bool isReleased = Request.Form["isReleased"].ToString() == "true";
                bool releaseSigned = !string.IsNullOrEmpty(Request.Form["releaseSigned"]) && Request.Form["releaseSigned"] == "true";

                // PRIDANÉ: Automatické nastavenie používateľa ak nie je zadané
                if (string.IsNullOrEmpty(modifiedBy))
                {
                    modifiedBy = currentUser;
                    System.Diagnostics.Debug.WriteLine($"📄 Automaticky nastavený modifiedBy: {modifiedBy}");
                }

                if (reviewerSigned && string.IsNullOrEmpty(reviewer))
                {
                    reviewer = currentUser;
                    System.Diagnostics.Debug.WriteLine($"📄 Automaticky nastavený reviewer: {reviewer}");
                }

                if (verifierSigned && string.IsNullOrEmpty(verifier))
                {
                    verifier = currentUser;
                    System.Diagnostics.Debug.WriteLine($"📄 Automaticky nastavený verifier: {verifier}");
                }

                if (releaseSigned && string.IsNullOrEmpty(releasedBy))
                {
                    releasedBy = currentUser;
                    System.Diagnostics.Debug.WriteLine($"📄 Automaticky nastavený releasedBy: {releasedBy}");
                }

                // Logování pro diagnostiku
                System.Diagnostics.Debug.WriteLine($"📄 SaveFormData: Zpracovávám formulář");
                System.Diagnostics.Debug.WriteLine($"📄 configId: {configId}");
                System.Diagnostics.Debug.WriteLine($"📄 reviewer: {reviewer}, reviewerSigned: {reviewerSigned}");
                System.Diagnostics.Debug.WriteLine($"📄 verifier: {verifier}, verifierSigned: {verifierSigned}");
                System.Diagnostics.Debug.WriteLine($"📄 releasedBy: {releasedBy}, releaseSigned: {releaseSigned}");

                // Validace nezbytných údajů
                if (configId <= 0)
                {
                    TempData["ErrorMessage"] = "Chybí ID konfiguračního listu.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(itemCode) || string.IsNullOrEmpty(itemName))
                {
                    TempData["ErrorMessage"] = "Kód a název položky jsou povinné.";
                    return RedirectToAction("Configlist", new { configId });
                }

                // Kontrola existence konfiguračního listu
                var config = await _context.ConfigurationSheets.FindAsync(configId);
                if (config == null)
                {
                    TempData["ErrorMessage"] = $"Konfigurační list s ID {configId} nebyl nalezen.";
                    return RedirectToAction("Index");
                }

                // Začíname transakciu pre konzistentnosť
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // ... váš existujúci kód pre ProgramItem a ProgramItemDetail zostáva rovnaký ...

                        // Uložení nebo aktualizace položky
                        ProgramItem item;
                        if (itemId > 0)
                        {
                            item = await _context.ProgramItems.FindAsync(itemId);
                            if (item != null)
                            {
                                item.ItemCode = itemCode;
                                item.ItemName = itemName;
                                item.ItemDescription = itemDescription;
                                _context.Entry(item).State = EntityState.Modified;
                            }
                            else
                            {
                                item = new ProgramItem
                                {
                                    ConfigId = configId,
                                    ItemCode = itemCode,
                                    ItemName = itemName,
                                    ItemDescription = itemDescription
                                };
                                _context.ProgramItems.Add(item);
                            }
                        }
                        else
                        {
                            item = new ProgramItem
                            {
                                ConfigId = configId,
                                ItemCode = itemCode,
                                ItemName = itemName,
                                ItemDescription = itemDescription
                            };
                            _context.ProgramItems.Add(item);
                        }

                        if (item.ItemId <= 0)
                        {
                            await _context.SaveChangesAsync();
                            itemId = item.ItemId;
                        }

                        // Detail handling...
                        ProgramItemDetail detail;
                        if (detailId > 0)
                        {
                            detail = await _context.ProgramItemDetails.FindAsync(detailId);
                            if (detail == null)
                            {
                                detail = await _context.ProgramItemDetails.FirstOrDefaultAsync(d => d.ItemId == itemId);
                            }
                        }
                        else
                        {
                            detail = await _context.ProgramItemDetails.FirstOrDefaultAsync(d => d.ItemId == itemId);
                        }

                        if (detail != null)
                        {
                            detail.Ppname = ppname;
                            detail.Ppnumber = ppnumber;
                            detail.ModifiedBy = modifiedBy;
                            detail.InitialVersionNumber = initialVersionNumber;
                            detail.DevelopmentTools = developmentTools;
                            detail.DevelopmentPc = developmentPc;
                            detail.Connections = connections;
                            detail.RelatedDocumentation = relatedDocumentation;
                            detail.Notes = notes;
                            detail.LastModifiedDate = DateTime.Now;
                            _context.Entry(detail).State = EntityState.Modified;
                        }
                        else
                        {
                            detail = new ProgramItemDetail
                            {
                                ItemId = itemId,
                                Ppname = ppname,
                                Ppnumber = ppnumber,
                                ModifiedBy = modifiedBy,
                                InitialVersionNumber = initialVersionNumber,
                                DevelopmentTools = developmentTools,
                                DevelopmentPc = developmentPc,
                                Connections = connections,
                                RelatedDocumentation = relatedDocumentation,
                                Notes = notes,
                                LastModifiedDate = DateTime.Now
                            };
                            _context.ProgramItemDetails.Add(detail);
                        }

                        await _context.SaveChangesAsync();
                        detailId = detail.DetailId;

                        // ZMENENÉ: Volanie helper metód s UserId
                        await SaveProgramReview(detailId, reviewForm, reviewResult, reviewer, reviewerSigned ? currentUserId : null, reviewDate);
                        await SaveProgramVerification(detailId, verificationForm, verificationResult, verifier, verifierSigned ? currentUserId : null, verificationDate);
                        await SaveProgramRelease(detailId, releasedBy, isReleased, releaseSigned ? currentUserId : null);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        System.Diagnostics.Debug.WriteLine("✅ Data úspěšně uložena");
                        return RedirectToAction("Index", new { configId });
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CHYBA: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                TempData["ErrorMessage"] = $"Došlo k chybě při ukládání dat: {ex.Message}";

                int configId = 0;
                int.TryParse(Request.Form["configId"], out configId);

                if (configId > 0)
                {
                    return RedirectToAction("Configlist", new { configId });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }

        // KROK 4: Aktualizované helper metódy s UserId
        private async Task SaveProgramReview(int detailId, string reviewForm, string reviewResult, string reviewer, Guid? reviewerUserId, string reviewDateStr)
        {
            DateOnly? reviewDate = null;
            if (!string.IsNullOrEmpty(reviewDateStr))
            {
                if (DateTime.TryParse(reviewDateStr, out DateTime tempDate))
                {
                    reviewDate = DateOnly.FromDateTime(tempDate);
                }
            }

            // PRIDANÉ: Získanie URL podpisu ak je používateľ podpísaný
            string reviewerSignatureUrl = null;
            if (reviewerUserId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.Details)
                    .FirstOrDefaultAsync(u => u.Id.ToString() == reviewerUserId.ToString());

                reviewerSignatureUrl = user?.Details?.SignatureImagePath;
                System.Diagnostics.Debug.WriteLine($"🔍 Získaný podpis pre reviewera: {reviewerSignatureUrl}");
            }

            var review = await _context.ProgramReviews.FirstOrDefaultAsync(r => r.DetailId == detailId);

            if (review != null)
            {
                review.ReviewForm = reviewForm;
                review.ReviewResult = reviewResult;
                review.Reviewer = reviewer;
                review.ReviewerUserId = reviewerUserId;
                review.ReviewerSignature = reviewerSignatureUrl; // PRIDANÉ
                review.ReviewDate = reviewDate;

                _context.Entry(review).State = EntityState.Modified;
                System.Diagnostics.Debug.WriteLine($"✏️ Aktualizace ProgramReview pre DetailId: {detailId}, UserId: {reviewerUserId}, Signature: {reviewerSignatureUrl}");
            }
            else if (!string.IsNullOrEmpty(reviewForm) || !string.IsNullOrEmpty(reviewResult) || !string.IsNullOrEmpty(reviewer) || reviewerUserId.HasValue)
            {
                var newReview = new ProgramReview
                {
                    DetailId = detailId,
                    ReviewForm = reviewForm,
                    ReviewResult = reviewResult,
                    Reviewer = reviewer,
                    ReviewerUserId = reviewerUserId,
                    ReviewerSignature = reviewerSignatureUrl, // PRIDANÉ
                    ReviewDate = reviewDate
                };

                _context.ProgramReviews.Add(newReview);
                System.Diagnostics.Debug.WriteLine($"🆕 Vytvoření nového ProgramReview pre DetailId: {detailId}, UserId: {reviewerUserId}, Signature: {reviewerSignatureUrl}");
            }
        }

        private async Task SaveProgramVerification(int detailId, string verificationForm, string verificationResult, string verifier, Guid? verifierUserId, string verificationDateStr)
        {
            DateOnly? verificationDate = null;
            if (!string.IsNullOrEmpty(verificationDateStr))
            {
                if (DateTime.TryParse(verificationDateStr, out DateTime tempDate))
                {
                    verificationDate = DateOnly.FromDateTime(tempDate);
                }
            }

            // PRIDANÉ: Získanie URL podpisu ak je používateľ podpísaný
            string verifierSignatureUrl = null;
            if (verifierUserId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.Details)
                    .FirstOrDefaultAsync(u => u.Id.ToString() == verifierUserId.ToString());

                verifierSignatureUrl = user?.Details?.SignatureImagePath;
                System.Diagnostics.Debug.WriteLine($"🔍 Získaný podpis pre verifier: {verifierSignatureUrl}");
            }

            var verification = await _context.ProgramVerifications.FirstOrDefaultAsync(v => v.DetailId == detailId);

            if (verification != null)
            {
                verification.ReviewForm = verificationForm;
                verification.ReviewResult = verificationResult;
                verification.Reviewer = verifier;
                verification.VerifierUserId = verifierUserId;
                verification.ReviewerSignature = verifierSignatureUrl; // PRIDANÉ
                verification.ReviewDate = verificationDate;

                _context.Entry(verification).State = EntityState.Modified;
                System.Diagnostics.Debug.WriteLine($"✏️ Aktualizace ProgramVerification pre DetailId: {detailId}, UserId: {verifierUserId}, Signature: {verifierSignatureUrl}");
            }
            else if (!string.IsNullOrEmpty(verificationForm) || !string.IsNullOrEmpty(verificationResult) || !string.IsNullOrEmpty(verifier) || verifierUserId.HasValue)
            {
                var newVerification = new ProgramVerification
                {
                    DetailId = detailId,
                    ReviewForm = verificationForm,
                    ReviewResult = verificationResult,
                    Reviewer = verifier,
                    VerifierUserId = verifierUserId,
                    ReviewerSignature = verifierSignatureUrl, // PRIDANÉ
                    ReviewDate = verificationDate
                };

                _context.ProgramVerifications.Add(newVerification);
                System.Diagnostics.Debug.WriteLine($"🆕 Vytvoření nového ProgramVerification pre DetailId: {detailId}, UserId: {verifierUserId}, Signature: {verifierSignatureUrl}");
            }
        }

        private async Task SaveProgramRelease(int detailId, string releasedBy, bool isReleased, Guid? releasedByUserId)
        {
            // PRIDANÉ: Získanie URL podpisu ak je používateľ podpísaný
            string releaseSignatureUrl = null;
            if (releasedByUserId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.Details)
                    .FirstOrDefaultAsync(u => u.Id.ToString() == releasedByUserId.ToString());

                releaseSignatureUrl = user?.Details?.SignatureImagePath;
                System.Diagnostics.Debug.WriteLine($"🔍 Získaný podpis pre release: {releaseSignatureUrl}");
            }

            var release = await _context.ProgramReleases.FirstOrDefaultAsync(r => r.DetailId == detailId);

            if (release != null)
            {
                release.ReleasedBy = releasedBy;
                release.IsReleased = isReleased;
                release.ReleasedByUserId = releasedByUserId;
                release.ReleaseSignature = releaseSignatureUrl; // PRIDANÉ
                if (isReleased)
                {
                    release.ReleasedDate = DateTime.Now;
                }

                _context.Entry(release).State = EntityState.Modified;
                System.Diagnostics.Debug.WriteLine($"✏️ Aktualizace ProgramRelease pre DetailId: {detailId}, UserId: {releasedByUserId}, Signature: {releaseSignatureUrl}");
            }
            else if (!string.IsNullOrEmpty(releasedBy) || releasedByUserId.HasValue || isReleased)
            {
                var newRelease = new ProgramRelease
                {
                    DetailId = detailId,
                    ReleasedBy = releasedBy,
                    IsReleased = isReleased,
                    ReleasedByUserId = releasedByUserId,
                    ReleaseSignature = releaseSignatureUrl, // PRIDANÉ
                    ReleasedDate = isReleased ? DateTime.Now : (DateTime?)null
                };

                _context.ProgramReleases.Add(newRelease);
                System.Diagnostics.Debug.WriteLine($"🆕 Vytvoření nového ProgramRelease pre DetailId: {detailId}, UserId: {releasedByUserId}, Signature: {releaseSignatureUrl}");
            }
        }



        // PRIDAJTE TIETO METÓDY DO VÁŠHO MEDZIOPERACNAKONTRÓLA KONTROLÉRA
        // (pridajte ich hneď za Delete metódy)

        [HttpGet]
        public async Task<IActionResult> Edit(int configId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📝 Načítavam konfiguračný list {configId} pre editáciu");

                // Načítanie konfiguračného listu s položkami
                var config = await _context.ConfigurationSheets
                    .Include(c => c.ProgramItems)
                    .FirstOrDefaultAsync(c => c.ConfigId == configId);

                if (config == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Konfiguračný list {configId} nebol nájdený");
                    TempData["ErrorMessage"] = $"Konfiguračný list s ID {configId} nebol nájdený.";
                    return RedirectToAction("Index");
                }

                // Vytvorenie view modelu s existujúcimi dátami
                var model = new ConfigurationSheetViewModel
                {
                    ConfigurationSheet = config,
                    ProgramItems = config.ProgramItems?.ToList() ?? new List<ProgramItem>()
                };

                System.Diagnostics.Debug.WriteLine($"📝 Editácia konfiguračného listu {configId} s {model.ProgramItems.Count} položkami");

                return View("Create", model); // Použijeme rovnaký view ako pre Create
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri načítaní pre editáciu: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                TempData["ErrorMessage"] = $"Nastala chyba pri načítaní: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ConfigurationSheetViewModel model, string action)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📝 Ukladám editáciu konfiguračného listu ID: {model.ConfigurationSheet?.ConfigId}");
                System.Diagnostics.Debug.WriteLine($"📝 Akcia: {action}");

                if (model.ProgramItems == null)
                {
                    model.ProgramItems = new List<ProgramItem>();
                }

                // Filtrácia prázdnych položiek
                var originalCount = model.ProgramItems.Count;
                model.ProgramItems = model.ProgramItems
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(item.ItemCode) &&
                        !string.IsNullOrWhiteSpace(item.ItemName))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"📝 Položky: {originalCount} -> {model.ProgramItems.Count} (po filtrácii)");

                // Odstránenie ModelState chýb pre filtrované položky
                var keysToRemove = ModelState.Keys
                    .Where(key => key.StartsWith("ProgramItems["))
                    .ToList();

                foreach (var key in keysToRemove)
                    ModelState.Remove(key);

                // Validácia filtrovaných položiek
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
                    System.Diagnostics.Debug.WriteLine("❌ Model nie je validný pri editácii");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"- {error.ErrorMessage}");
                    }
                    return View("Create", model);
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Aktualizácia konfiguračného listu
                        var existingConfig = await _context.ConfigurationSheets.FindAsync(model.ConfigurationSheet.ConfigId);
                        if (existingConfig == null)
                        {
                            TempData["ErrorMessage"] = "Konfiguračný list nebol nájdený.";
                            return RedirectToAction("Index");
                        }

                        // Aktualizácia základných údajov
                        existingConfig.Apvname = model.ConfigurationSheet.Apvname;
                        existingConfig.Apvnumber = model.ConfigurationSheet.Apvnumber;
                        existingConfig.ContractNumber = model.ConfigurationSheet.ContractNumber;
                        existingConfig.OrderNumber = model.ConfigurationSheet.OrderNumber;
                        existingConfig.Processor = model.ConfigurationSheet.Processor;
                        existingConfig.RelatedDocumentation = model.ConfigurationSheet.RelatedDocumentation;
                        existingConfig.RelatedHwsw = model.ConfigurationSheet.RelatedHwsw;
                     

                        _context.Entry(existingConfig).State = EntityState.Modified;
                        System.Diagnostics.Debug.WriteLine($"✏️ Aktualizovaný konfiguračný list {existingConfig.ConfigId}");

                        // 2. Spracovanie programových položiek
                        var existingItems = await _context.ProgramItems
                            .Where(p => p.ConfigId == model.ConfigurationSheet.ConfigId)
                            .ToListAsync();

                        System.Diagnostics.Debug.WriteLine($"📝 Existujúce položky: {existingItems.Count}");

                        // Odstránenie položiek ktoré už nie sú v modeli (majú ItemId ale nie sú v novom modeli)
                        var itemsToDelete = existingItems
                            .Where(existing => !model.ProgramItems.Any(newItem => newItem.ItemId == existing.ItemId && newItem.ItemId > 0))
                            .ToList();

                        if (itemsToDelete.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"🗑️ Mazanie {itemsToDelete.Count} položiek");
                            foreach (var itemToDelete in itemsToDelete)
                            {
                                System.Diagnostics.Debug.WriteLine($"🗑️ Mažem položku ID: {itemToDelete.ItemId} - {itemToDelete.ItemCode}");
                            }
                            _context.ProgramItems.RemoveRange(itemsToDelete);
                        }

                        // Aktualizácia/pridanie položiek
                        foreach (var item in model.ProgramItems)
                        {
                            item.ConfigId = model.ConfigurationSheet.ConfigId;

                            if (item.ItemId > 0)
                            {
                                // Aktualizácia existujúcej položky
                                var existingItem = existingItems.FirstOrDefault(e => e.ItemId == item.ItemId);
                                if (existingItem != null)
                                {
                                    existingItem.ItemCode = item.ItemCode;
                                    existingItem.ItemName = item.ItemName;
                                    existingItem.ItemDescription = item.ItemDescription;
                                    _context.Entry(existingItem).State = EntityState.Modified;
                                    System.Diagnostics.Debug.WriteLine($"✏️ Aktualizovaná položka ID: {item.ItemId} - {item.ItemCode}");
                                }
                            }
                            else
                            {
                                // Pridanie novej položky
                                _context.ProgramItems.Add(item);
                                System.Diagnostics.Debug.WriteLine($"🆕 Pridaná nová položka: {item.ItemCode}");
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        System.Diagnostics.Debug.WriteLine($"✅ Konfiguračný list {model.ConfigurationSheet.ConfigId} úspešne aktualizovaný");
                        TempData["SuccessMessage"] = "Konfiguračný list bol úspešne aktualizovaný.";

                        // Presmerovanie podľa akcie
                        if (action == "save")
                        {
                            return RedirectToAction("Edit", new { configId = model.ConfigurationSheet.ConfigId });
                        }
                        else // action == "continue"
                        {
                            return RedirectToAction("Configlist", new { configId = model.ConfigurationSheet.ConfigId });
                        }
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CHYBA pri editácii: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                TempData["ErrorMessage"] = $"Nastala chyba pri ukladaní: {ex.Message}";
                ModelState.AddModelError("", $"Nastala chyba pri ukladaní: {ex.Message}");
                return View("Create", model);
            }
        }

        // PRIDAJTE TÚTO METÓDU DO VÁŠHO MEDZIOPERACNAKONTRÓLA KONTROLÉRA
        // (pridajte ju hneď za Edit metódy)

        [HttpGet]
        public async Task<IActionResult> Detail(int configId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"👁️ Načítavam detail konfiguračného listu {configId}");

                // Načítanie konfiguračného listu s položkami a všetkými súvisiacimi dátami
                var config = await _context.ConfigurationSheets
                    .Include(c => c.ProgramItems)
                    .FirstOrDefaultAsync(c => c.ConfigId == configId);

                if (config == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Konfiguračný list {configId} nebol nájdený");
                    TempData["ErrorMessage"] = $"Konfiguračný list s ID {configId} nebol nájdený.";
                    return RedirectToAction("Index");
                }

                // Načítanie detailov pre všetky programové položky
                var programItemDetails = new List<ProgramItemDetail>();
                var programReviews = new List<ProgramReview>();
                var programVerifications = new List<ProgramVerification>();
                var programReleases = new List<ProgramRelease>();

                foreach (var item in config.ProgramItems)
                {
                    // Načítanie detailu položky
                    var detail = await _context.ProgramItemDetails
                        .FirstOrDefaultAsync(d => d.ItemId == item.ItemId);

                    if (detail != null)
                    {
                        programItemDetails.Add(detail);

                        // Načítanie review s podpisom
                        var review = await _context.ProgramReviews
                            .Include(r => r.ReviewerUser)
                            .ThenInclude(u => u.Details)
                            .FirstOrDefaultAsync(r => r.DetailId == detail.DetailId);

                        if (review != null)
                        {
                            // Nastavenie podpisu
                            if (review.ReviewerUser?.Details?.SignatureImagePath != null)
                            {
                                review.ReviewerSignature = review.ReviewerUser.Details.SignatureImagePath;
                            }
                            programReviews.Add(review);
                        }

                        // Načítanie verification s podpisom
                        var verification = await _context.ProgramVerifications
                            .Include(v => v.VerifierUser)
                            .ThenInclude(u => u.Details)
                            .FirstOrDefaultAsync(v => v.DetailId == detail.DetailId);

                        if (verification != null)
                        {
                            // Nastavenie podpisu
                            if (verification.VerifierUser?.Details?.SignatureImagePath != null)
                            {
                                verification.ReviewerSignature = verification.VerifierUser.Details.SignatureImagePath;
                            }
                            programVerifications.Add(verification);
                        }

                        // Načítanie release s podpisom
                        var release = await _context.ProgramReleases
                            .Include(r => r.ReleasedByUser)
                            .ThenInclude(u => u.Details)
                            .FirstOrDefaultAsync(r => r.DetailId == detail.DetailId);

                        if (release != null)
                        {
                            // Nastavenie podpisu
                            if (release.ReleasedByUser?.Details?.SignatureImagePath != null)
                            {
                                release.ReleaseSignature = release.ReleasedByUser.Details.SignatureImagePath;
                            }
                            programReleases.Add(release);
                        }
                    }
                }

                // Vytvorenie detail view modelu
                var model = new ConfigurationSheetDetailViewModel
                {
                    ConfigurationSheet = config,
                    ProgramItems = config.ProgramItems?.ToList() ?? new List<ProgramItem>(),
                    ProgramItemDetails = programItemDetails,
                    ProgramReviews = programReviews,
                    ProgramVerifications = programVerifications,
                    ProgramReleases = programReleases
                };

                System.Diagnostics.Debug.WriteLine($"👁️ Detail konfiguračného listu {configId} načítaný s {model.ProgramItems.Count} položkami");

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Chyba pri načítaní detailu: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                TempData["ErrorMessage"] = $"Nastala chyba pri načítaní detailu: {ex.Message}";
                return RedirectToAction("Index");
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

                // Začíname transakciu pre bezpečné mazanie
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // KROK 1: Získame všetky DetailId ktoré budeme mazať
                        var detailIds = await _context.ProgramItemDetails
                            .Where(d => config.ProgramItems.Select(p => p.ItemId).Contains(d.ItemId))
                            .Select(d => d.DetailId)
                            .ToListAsync();

                        System.Diagnostics.Debug.WriteLine($"🗑️ Našiel som {detailIds.Count} detailov na vymazanie");

                        if (detailIds.Any())
                        {
                            // KROK 2: Odstrániť ProgramReviews (najskôr závislé tabuľky)
                            var reviews = await _context.ProgramReviews
                                .Where(r => detailIds.Contains(r.DetailId))
                                .ToListAsync();

                            if (reviews.Any())
                            {
                                _context.ProgramReviews.RemoveRange(reviews);
                                System.Diagnostics.Debug.WriteLine($"🗑️ Odstraňujem {reviews.Count} ProgramReviews");
                            }

                            // KROK 3: Odstrániť ProgramVerifications
                            var verifications = await _context.ProgramVerifications
                                .Where(v => detailIds.Contains(v.DetailId))
                                .ToListAsync();

                            if (verifications.Any())
                            {
                                _context.ProgramVerifications.RemoveRange(verifications);
                                System.Diagnostics.Debug.WriteLine($"🗑️ Odstraňujem {verifications.Count} ProgramVerifications");
                            }

                            // KROK 4: Odstrániť ProgramReleases
                            var releases = await _context.ProgramReleases
                                .Where(r => detailIds.Contains(r.DetailId))
                                .ToListAsync();

                            if (releases.Any())
                            {
                                _context.ProgramReleases.RemoveRange(releases);
                                System.Diagnostics.Debug.WriteLine($"🗑️ Odstraňujem {releases.Count} ProgramReleases");
                            }

                            // KROK 5: Uložiť zmeny pre závislé tabuľky
                            await _context.SaveChangesAsync();

                            // KROK 6: Teraz môžeme bezpečne odstrániť ProgramItemDetails
                            var details = await _context.ProgramItemDetails
                                .Where(d => detailIds.Contains(d.DetailId))
                                .ToListAsync();

                            if (details.Any())
                            {
                                _context.ProgramItemDetails.RemoveRange(details);
                                System.Diagnostics.Debug.WriteLine($"🗑️ Odstraňujem {details.Count} ProgramItemDetails");
                                await _context.SaveChangesAsync();
                            }
                        }

                        // KROK 7: Odstrániť ProgramItems
                        if (config.ProgramItems.Any())
                        {
                            _context.ProgramItems.RemoveRange(config.ProgramItems);
                            System.Diagnostics.Debug.WriteLine($"🗑️ Odstraňujem {config.ProgramItems.Count} ProgramItems");
                            await _context.SaveChangesAsync();
                        }

                        // KROK 8: Nakoniec odstrániť ConfigurationSheet
                        _context.ConfigurationSheets.Remove(config);
                        await _context.SaveChangesAsync();

                        // Potvrdenie transakcie
                        await transaction.CommitAsync();

                        System.Diagnostics.Debug.WriteLine("✅ Konfiguračný list úspešne vymazaný");
                        TempData["SuccessMessage"] = "Konfiguračný list bol úspešne vymazaný.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception)
                    {
                        // Rollback pri chybe
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CHYBA pri mazaní: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                TempData["ErrorMessage"] = $"Nastala chyba pri vymazávaní: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult ExamRecord() => View();
    }
}
