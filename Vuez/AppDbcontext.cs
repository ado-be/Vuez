using Microsoft.EntityFrameworkCore;

namespace vuez
{
    public class AppDbContext : DbContext
    {
        public DbSet<Indicators> Indicators { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Nastavenie presnosti pre Price
            modelBuilder.Entity<Indicators>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Seed data - pridanie základných údajov do databázy
            modelBuilder.Entity<Indicators>().HasData(
                new Indicators { Id = 1, Name = "Indicator1", Price = 100.0m },
                new Indicators { Id = 2, Name = "Indicator2", Price = 200.0m }
            );
        }
    }
}
