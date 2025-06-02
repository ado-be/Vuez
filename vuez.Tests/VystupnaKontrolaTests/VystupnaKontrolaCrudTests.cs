using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using vuez;                     // AppDbContext leží práve tu
using vuez.Models;              // entita VystupnaKontrola
using vuez.Tests.Data;          // InMemoryDbContextFactory

namespace vuez.Tests.VystupnaKontrolaTests
{
    public class VystupnaKontrolaCrudTests
    {
        [Fact]
        public async Task CanAddAndRetrieveVystupnaKontrola()
        {
            // ARRANGE: Vytvoríme nový in-memory DbContext
            using var context = InMemoryDbContextFactory.Create();

            // Overíme, že tabuľka je prázdna
            Assert.Empty(await context.VystupnaKontrola.ToListAsync());

            // Vytvoríme nový záznam typu VystupnaKontrola
            var novy = new VystupnaKontrola
            {
                // Id je PK s Identity; v InMemory ho môžeme buď nastaviť ručne, alebo nechať EF, 
                // ale v InMemory provider musí byť Id nastavené, pretože neimplementuje Identity autoincrement. 
                // Ak vy NEpotrebujete Id čítať pred uložením, môžete ho vynechať a získa sa default 0 – EF si ho priradí ako 1.
                // Tu ho nevieme generovať – preto explicitne:
                Id = 1,
                CisloProtokolu = 123,
                NazovVyrobku = "ProduktTest",
                Objednavatel = "FirmaTest",
                ZakazkoveCislo = "ZC-001",
                KontrolaPodla = "Norma1",
                KompletnostTechaVyr = "OK",
                KompletnostKontrolnych = "OK",
                KompletnostSprievodnej = "OK",
                Pripravenostkexp = "OK",
                Poznamky = "Žiadne",
                Miesto = "SkladA",
                Datum = new DateTime(2025, 6, 1),
                PodpisManagerUrl = "http://example.com/podpisManager.png",
                PodpisTechnikUrl = "http://example.com/podpisTechnik.png"
            };

            // ACT: Uložíme ho do databázy
            await context.VystupnaKontrola.AddAsync(novy);
            await context.SaveChangesAsync();

            // ASSERT: Skontrolujeme, že sa dá načítať
            var saved = await context.VystupnaKontrola
                                     .FirstOrDefaultAsync(v => v.ZakazkoveCislo == "ZC-001");
            Assert.NotNull(saved);

            Assert.Equal("ProduktTest", saved!.NazovVyrobku);
            Assert.Equal("FirmaTest", saved.Objednavatel);
            Assert.Equal(123, saved.CisloProtokolu);
            Assert.Equal("Norma1", saved.KontrolaPodla);
            Assert.Equal("OK", saved.KompletnostTechaVyr);
            Assert.Equal("OK", saved.KompletnostKontrolnych);
            Assert.Equal("OK", saved.KompletnostSprievodnej);
            Assert.Equal("OK", saved.Pripravenostkexp);
            Assert.Equal("Žiadne", saved.Poznamky);
            Assert.Equal("SkladA", saved.Miesto);
            Assert.Equal(new DateTime(2025, 6, 1), saved.Datum);
            Assert.Equal("http://example.com/podpisManager.png", saved.PodpisManagerUrl);
            Assert.Equal("http://example.com/podpisTechnik.png", saved.PodpisTechnikUrl);
        }

        [Fact]
        public async Task RetrievingNonExistentVystupnaKontrolaReturnsNull()
        {
            // ARRANGE: nový kontext
            using var context = InMemoryDbContextFactory.Create();

            // ACT: pokus o načítanie záznamu, ktorý neexistuje
            var nic = await context.VystupnaKontrola.FindAsync(42);

            // ASSERT: Nula
            Assert.Null(nic);
        }

        [Fact]
        public async Task CanUpdateVystupnaKontrola()
        {
            // ARRANGE: Nový kontext a vložený záznam
            using var context = InMemoryDbContextFactory.Create();

            var orig = new VystupnaKontrola
            {
                Id = 5,
                CisloProtokolu = 555,
                NazovVyrobku = "PôvodnýNázov",
                Objednavatel = "FirmaOld",
                ZakazkoveCislo = "ZC-005"
            };
            await context.VystupnaKontrola.AddAsync(orig);
            await context.SaveChangesAsync();

            // ACT: Načítajme ho, upravme a uložme
            var toUpdate = await context.VystupnaKontrola.FirstAsync(v => v.Id == 5);
            toUpdate.NazovVyrobku = "NovýNázov";
            toUpdate.Objednavatel = "FirmaNew";
            toUpdate.PodpisManagerUrl = "http://new.podpis.png";
            await context.SaveChangesAsync();

            // ASSERT: Prečítame ho opäť a skontrolujeme zmenu
            var updated = await context.VystupnaKontrola.FindAsync(5);
            Assert.NotNull(updated);
            Assert.Equal("NovýNázov", updated!.NazovVyrobku);
            Assert.Equal("FirmaNew", updated.Objednavatel);
            Assert.Equal("http://new.podpis.png", updated.PodpisManagerUrl);
        }

        [Fact]
        public async Task CanDeleteVystupnaKontrola()
        {
            // ARRANGE: Nový kontext a vložený záznam
            using var context = InMemoryDbContextFactory.Create();

            var orig = new VystupnaKontrola
            {
                Id = 10,
                CisloProtokolu = 1010,
                NazovVyrobku = "K Mazaniu",
                Objednavatel = "FirmaDel",
                ZakazkoveCislo = "ZC-010"
            };
            await context.VystupnaKontrola.AddAsync(orig);
            await context.SaveChangesAsync();

            // Overenie, že záznam existuje
            Assert.NotNull(await context.VystupnaKontrola.FindAsync(10));

            // ACT: Vymažeme ho
            context.VystupnaKontrola.Remove(orig);
            await context.SaveChangesAsync();

            // ASSERT: Nemal by existovať
            var deleted = await context.VystupnaKontrola.FindAsync(10);
            Assert.Null(deleted);
        }
    }
}
