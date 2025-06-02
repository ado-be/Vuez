using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using vuez;                    // AppDbContext leží práve pod namespace "vuez"
using vuez.Models;             // entita VstupnaKontrola (vuez.Models)
using vuez.Tests.Data;         // InMemoryDbContextFactory

namespace vuez.Tests.VstupnaKontrolaTests
{
    public class VstupnaKontrolaCrudTests
    {
        [Fact]
        public async Task CanAddAndRetrieveVstupnaKontrola()
        {
            // ARRANGE: nový In-Memory DbContext
            using var context = InMemoryDbContextFactory.Create();

            // Overenie, že tabuľka VstupnaKontrola je na začiatku prázdna
            Assert.Empty(await context.VstupnaKontrola.ToListAsync());

            // Vytvorenie novej entity VstupnaKontrola
            var nova = new VstupnaKontrola
            {
                Id = 1,  // V In-Memory provideri EF Core neposkytuje auto-increment, ID nastavujeme ručne
                CisloProtokolu = 10,
                NazovVyrobku = "TestProduct",
                Dodavatel = "TestSupplier",
                ZakazkoveCislo = "ZK-100",
                KontrolaPodla = "NormaA",
                SpravnostDodavky = "OK",
                ZnacenieMaterialu = " označené",
                CistotaPovrchu = "čisto",
                Balenie = "kartón",
                Poskodenie = "žiadne",
                IneKroky = "postup1",
                Poznamky = "poznamka1",
                SuborCesta = @"C:\docs\test.pdf",
                Miesto = "Sklad1",
                Datum = new DateTime(2025, 7, 1),
                PodpisManagerUrl = "http://url.com/pm.png",
                PodpisTechnikUrl = "http://url.com/pt.png"
            };

            // ACT: pridanie a uloženie do in-memory DB
            await context.VstupnaKontrola.AddAsync(nova);
            await context.SaveChangesAsync();

            // ASSERT: načítanie podľa ZakazkoveCislo a overenie vlastností
            var saved = await context.VstupnaKontrola
                                     .FirstOrDefaultAsync(v => v.ZakazkoveCislo == "ZK-100");
            Assert.NotNull(saved);
            Assert.Equal("TestProduct", saved!.NazovVyrobku);
            Assert.Equal("TestSupplier", saved.Dodavatel);
            Assert.Equal(10, saved.CisloProtokolu);
            Assert.Equal("NormaA", saved.KontrolaPodla);
            Assert.Equal("OK", saved.SpravnostDodavky);
            Assert.Equal(" označené", saved.ZnacenieMaterialu);
            Assert.Equal("čisto", saved.CistotaPovrchu);
            Assert.Equal("kartón", saved.Balenie);
            Assert.Equal("žiadne", saved.Posko­denie);
            Assert.Equal("postup1", saved.IneKroky);
            Assert.Equal("poznamka1", saved.Poznamky);
            Assert.Equal(@"C:\docs\test.pdf", saved.SuborCesta);
            Assert.Equal("Sklad1", saved.Miesto);
            Assert.Equal(new DateTime(2025, 7, 1), saved.Datum);
            Assert.Equal("http://url.com/pm.png", saved.PodpisManagerUrl);
            Assert.Equal("http://url.com/pt.png", saved.PodpisTechnikUrl);
        }

        [Fact]
        public async Task RetrievingNonExistentVstupnaKontrolaReturnsNull()
        {
            // ARRANGE: nový kontext
            using var context = InMemoryDbContextFactory.Create();

            // ACT: pokus o načítanie neexistujúceho ID
            var nic = await context.VstupnaKontrola.FindAsync(999);

            // ASSERT: pretože tam nič nie je, vráti sa null
            Assert.Null(nic);
        }

        [Fact]
        public async Task CanUpdateVstupnaKontrola()
        {
            // ARRANGE: nový kontext a vloženie jedného záznamu
            using var context = InMemoryDbContextFactory.Create();

            var orig = new VstupnaKontrola
            {
                Id = 2,
                CisloProtokolu = 20,
                NazovVyrobku = "OldProduct",
                Dodavatel = "OldSupplier",
                ZakazkoveCislo = "ZK-200"
                // ostatné vlastnosti necháme null alebo default
            };
            await context.VstupnaKontrola.AddAsync(orig);
            await context.SaveChangesAsync();

            // ACT: načítanie, úprava a uloženie zmien
            var toUpdate = await context.VstupnaKontrola.FirstAsync(v => v.Id == 2);
            toUpdate.NazovVyrobku = "NewProduct";
            toUpdate.Dodavatel = "NewSupplier"; 
            // Skutočný názov vlastnosti je "Dodavatel", takže:
            toUpdate.Dodavatel = "NewSupplier";
            toUpdate.PodpisTechnikUrl = "http://url.com/updated_pt.png";
            await context.SaveChangesAsync();

            // ASSERT: načítame opäť a overíme, že sa zmenilo
            var updated = await context.VstupnaKontrola.FindAsync(2);
            Assert.NotNull(updated);
            Assert.Equal("NewProduct", updated!.NazovVyrobku);
            Assert.Equal("NewSupplier", updated.Dodavatel);
            Assert.Equal("http://url.com/updated_pt.png", updated.PodpisTechnikUrl);
        }

        [Fact]
        public async Task CanDeleteVstupnaKontrola()
        {
            // ARRANGE: nový kontext a vloženie jedného záznamu
            using var context = InMemoryDbContextFactory.Create();

            var orig = new VstupnaKontrola
            {
                Id = 3,
                CisloProtokolu = 30,
                NazovVyrobku = "ToDelete",
                Dodavatel = "SupplierDel",
                ZakazkoveCislo = "ZK-300"
            };
            await context.VstupnaKontrola.AddAsync(orig);
            await context.SaveChangesAsync();

            // Overenie, že záznam tam reálne je
            var beforeDelete = await context.VstupnaKontrola.FindAsync(3);
            Assert.NotNull(beforeDelete);

            // ACT: odstráníme ho a uložíme
            context.VstupnaKontrola.Remove(orig);
            await context.SaveChangesAsync();

            // ASSERT: už by tam nemal byť
            var deleted = await context.VstupnaKontrola.FindAsync(3);
            Assert.Null(deleted);
        }
    }
}
