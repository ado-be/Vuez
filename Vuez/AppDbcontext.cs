using Microsoft.EntityFrameworkCore;
using vuez.Models;
using System;

namespace vuez
{
    public class AppDbContext : DbContext
    {
        public DbSet<Indicators> Indicators { get; set; }
        public DbSet<PdfDocument> PdfDocuments { get; set; }
        public DbSet<VstupnaKontrola> VstupnaKontrola { get; set; }
        public DbSet<VystupnaKontrola> VystupnaKontrola { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }

        
        public DbSet<ConfigurationSheet> ConfigurationSheets { get; set; }
        public DbSet<ProgramItem> ProgramItems { get; set; }
        public DbSet<ProgramItemDetail> ProgramItemDetails { get; set; }
        public DbSet<DistributionList> DistributionLists { get; set; }
        public DbSet<PlannedTest> PlannedTests { get; set; }
        public DbSet<ProgramReview> ProgramReviews { get; set; }
        public DbSet<ProgramVerification> ProgramVerifications { get; set; }
        public DbSet<RepeatedTest> RepeatedTests { get; set; }
        public DbSet<ProgramRelease> ProgramReleases { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VstupnaKontrola>().ToTable("VstupnaKontrola");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles23");
            modelBuilder.Entity<UserDetails>().ToTable("UserDetails");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Details)
                .WithOne(d => d.User)
                .HasForeignKey<UserDetails>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDetails>()
                .Property(u => u.SignatureImagePath)
                .IsRequired(false);

            // ✅ Pevné ID-čka (statické, aby sa migrácie nemenili)
            var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            // ✅ Rola
            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = adminRoleId,
                RoleName = "Admin"
            });

            // ✅ Admin používateľ (s BCRYPT hash heslom)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminUserId,
                UserName = "admin",
                PasswordHash = hashedPassword,
                RoleId = adminRoleId
            });

            // ✅ Indikátory
            modelBuilder.Entity<Indicators>().HasData(
                new Indicators { Id = 1, Name = "Indicator1", Type = "TypeA", Producer = "Producer1", List_Num = "LN001" },
                new Indicators { Id = 2, Name = "Indicator2", Type = "TypeB", Producer = "Producer2", List_Num = "LN002" }
            );
        }
    }
}
