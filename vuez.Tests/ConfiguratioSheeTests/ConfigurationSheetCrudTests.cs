using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using vuez;                     // AppDbContext
using vuez.Models;              // ConfigurationSheet, ProgramItem, atď.
using vuez.Tests.Data;          // InMemoryDbContextFactory

namespace vuez.Tests.ConfigurationSheetTests
{
    public class ConfigurationSheetCrudTests
    {
        [Fact]
        public async Task CanAddAndRetrieveConfigurationSheet()
        {
            // ARRANGE: Vytvoríme nový in-memory DbContext
            using var context = InMemoryDbContextFactory.Create();

            // Overíme, že zatiaľ nie sú žiadne ConfigurationSheets
            Assert.Empty(await context.ConfigurationSheets.ToListAsync());

            // Vytvoríme nový ConfigurationSheet
            var newSheet = new ConfigurationSheet
            {
                // ConfigId v InMemory provideri zadáte ručne
                ConfigId = 1,
                Apvname = "APV Test",
                Apvnumber = "APV-001",
                ContractNumber = "CON-123",
                OrderNumber = "ORDER-789",
                Processor = "ProcessorX",
                RelatedHwsw = "HW/SW 1.0",
                RelatedDocumentation = "Doc1.pdf",
                CreatedDate = DateTime.UtcNow
                // ProgramItems ostanú prázdne, budú testované samostatne
            };

            // ACT: vložíme a uložime do databázy
            await context.ConfigurationSheets.AddAsync(newSheet);
            await context.SaveChangesAsync();

            // ASSERT: Načítame späť a overíme hodnoty
            var saved = await context.ConfigurationSheets
                                     .FirstOrDefaultAsync(cs => cs.Apvnumber == "APV-001");
            Assert.NotNull(saved);
            Assert.Equal("APV Test", saved!.Apvname);
            Assert.Equal("CON-123", saved.ContractNumber);
            Assert.Equal("ORDER-789", saved.OrderNumber);
            Assert.Equal("ProcessorX", saved.Processor);
            Assert.Equal("HW/SW 1.0", saved.RelatedHwsw);
            Assert.Equal("Doc1.pdf", saved.RelatedDocumentation);
            Assert.NotNull(saved.CreatedDate);
        }

        [Fact]
        public async Task RetrievingNonExistentConfigurationSheetReturnsNull()
        {
            // ARRANGE: nový in-memory kontext
            using var context = InMemoryDbContextFactory.Create();

            // ACT: pokus o načítanie neexistujúcej entitiy
            var none = await context.ConfigurationSheets.FindAsync(999);

            // ASSERT: očakávame null
            Assert.Null(none);
        }

        [Fact]
        public async Task CanUpdateConfigurationSheet()
        {
            // ARRANGE: nový in-memory kontext a vloženie existujúcej ConfigurationSheet
            using var context = InMemoryDbContextFactory.Create();

            var orig = new ConfigurationSheet
            {
                ConfigId = 2,
                Apvname = "APV Old",
                Apvnumber = "APV-OLD",
                OrderNumber = "ORDER-OLD"
            };
            await context.ConfigurationSheets.AddAsync(orig);
            await context.SaveChangesAsync();

            // ACT: načítame rovnaký objekt, upravíme a uložime
            var toUpdate = await context.ConfigurationSheets.FirstAsync(cs => cs.ConfigId == 2);
            toUpdate.Apvname = "APV New";
            toUpdate.OrderNumber = "ORDER-NEW";
            toUpdate.Processor = "ProcNew";
            await context.SaveChangesAsync();

            // ASSERT: Prečítame ho znovu a skontrolujeme zmenu
            var updated = await context.ConfigurationSheets.FindAsync(2);
            Assert.NotNull(updated);
            Assert.Equal("APV New", updated!.Apvname);
            Assert.Equal("ORDER-NEW", updated.OrderNumber);
            Assert.Equal("ProcNew", updated.Processor);
        }

        [Fact]
        public async Task CanDeleteConfigurationSheet()
        {
            // ARRANGE: nový in-memory kontext a vloženie záznamu
            using var context = InMemoryDbContextFactory.Create();

            var orig = new ConfigurationSheet
            {
                ConfigId = 3,
                Apvname = "ToDelete",
                Apvnumber = "DELETE-001",
                OrderNumber = "DEL-123"
            };
            await context.ConfigurationSheets.AddAsync(orig);
            await context.SaveChangesAsync();

            // Overíme, že záznam existuje
            var before = await context.ConfigurationSheets.FindAsync(3);
            Assert.NotNull(before);

            // ACT: vymažeme ho a uložíme
            context.ConfigurationSheets.Remove(orig);
            await context.SaveChangesAsync();

            // ASSERT: už by tam nemal byť
            var deleted = await context.ConfigurationSheets.FindAsync(3);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task CanAddConfigurationSheetWithProgramItems()
        {
            // ARRANGE: nový in-memory kontext
            using var context = InMemoryDbContextFactory.Create();

            // Vytvoríme ConfigurationSheet spolu s dvoma ProgramItem
            var sheet = new ConfigurationSheet
            {
                ConfigId = 4,
                Apvname = "APV With Items",
                Apvnumber = "APV-004",
                OrderNumber = "ORDER-004",
                ProgramItems = new[]
                {
                    new ProgramItem
                    {
                        ItemId = 10,
                        ItemCode = "ITEM-10",
                        ItemName = "ItemName10",
                        // ConfigId sa v In-Memory musi zhodovať s ConfigId rodiča
                        ConfigId = 4
                    },
                    new ProgramItem
                    {
                        ItemId = 11,
                        ItemCode = "ITEM-11",
                        ItemName = "ItemName11",
                        ConfigId = 4
                    }
                }
            };

            // ACT: pridáme sheet aj s ProgramItems v jednom kroku
            await context.ConfigurationSheets.AddAsync(sheet);
            await context.SaveChangesAsync();

            // ASSERT: Načítame sheet vrátane ProgramItems
            var loaded = await context.ConfigurationSheets
                                       .Include(cs => cs.ProgramItems)
                                       .FirstOrDefaultAsync(cs => cs.ConfigId == 4);
            Assert.NotNull(loaded);
            Assert.Equal(2, loaded!.ProgramItems.Count);

            // Skontrolujeme aspoň jedno z ProgramItem
            var firstItem = loaded.ProgramItems.FirstOrDefault(pi => pi.ItemCode == "ITEM-10");
            Assert.NotNull(firstItem);
            Assert.Equal("ItemName10", firstItem!.ItemName);
            Assert.Equal(4, firstItem.ConfigId);
        }
    }
}
