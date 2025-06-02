using System;
using Microsoft.EntityFrameworkCore;
using vuez; // namespace, kde je AppDbContext

namespace vuez.Tests.Data
{
    public static class InMemoryDbContextFactory
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            // (Neprávne) seed data – tu by ste mohli predvyplniť, ak chcete:
            // context.VystupnaKontrola.Add(new VystupnaKontrola { … });
            // context.SaveChanges();

            return context;
        }
    }
}
