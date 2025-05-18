using Microsoft.EntityFrameworkCore;

namespace vuez
{
    public class AppDbContext : DbContext
    {
        public DbSet<Indicators> Indicators { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data - pridanie základných údajov do databázy
            modelBuilder.Entity<Indicators>().HasData(
                new Indicators { Id = 1, Name = "Indicator1", Type = "TypeA", Producer = "Producer1", List_Num = "LN001" },
                new Indicators { Id = 2, Name = "Indicator2", Type = "TypeB", Producer = "Producer2", List_Num = "LN002" }
            );
        }
    }
}
