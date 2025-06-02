using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using vuez;                // AppDbContext
using vuez.Models;         // User, Role, UserDetails
using vuez.Tests.Data;     // InMemoryDbContextFactory

namespace vuez.Tests.UserTests
{
    public class UserCrudTests
    {
        [Fact]
        public async Task CanAddAndRetrieveRole()
        {
            // ARRANGE: nový in-memory DbContext
            using var context = InMemoryDbContextFactory.Create();

            // Overíme, že zatiaľ nie sú žiadne Role
            Assert.Empty(await context.Roles.ToListAsync());

            // Vytvoríme novú rolu
            var newRole = new Role
            {
                RoleName = "Technik"
            };

            // ACT: vložíme a uložime do databázy
            await context.Roles.AddAsync(newRole);
            await context.SaveChangesAsync();

            // ASSERT: Načítame späť a overíme hodnoty
            var savedRole = await context.Roles
                                         .FirstOrDefaultAsync(r => r.RoleName == "Technik");
            Assert.NotNull(savedRole);
            Assert.Equal("Technik", savedRole!.RoleName);
            Assert.NotEqual(Guid.Empty, savedRole.Id);
        }

        [Fact]
        public async Task RetrievingNonExistentRoleReturnsNull()
        {
            // ARRANGE: nový in-memory kontext
            using var context = InMemoryDbContextFactory.Create();

            // ACT: pokus o načítanie neexistujúcej Role
            var none = await context.Roles.FindAsync(Guid.NewGuid());

            // ASSERT: očakávame null
            Assert.Null(none);
        }

        [Fact]
        public async Task CanUpdateRole()
        {
            // ARRANGE: nový in-memory kontext a vloženie existujúcej Role
            using var context = InMemoryDbContextFactory.Create();

            var origRole = new Role
            {
                RoleName = "OldRole"
            };
            await context.Roles.AddAsync(origRole);
            await context.SaveChangesAsync();

            // ACT: načítame rovnakú rolu, upravíme a uložime
            var toUpdate = await context.Roles.FirstAsync(r => r.Id == origRole.Id);
            toUpdate.RoleName = "NewRole";
            await context.SaveChangesAsync();

            // ASSERT: Prečítame ho znovu a skontrolujeme zmenu
            var updated = await context.Roles.FindAsync(origRole.Id);
            Assert.NotNull(updated);
            Assert.Equal("NewRole", updated!.RoleName);
        }

        [Fact]
        public async Task CanDeleteRole()
        {
            // ARRANGE: nový in-memory kontext a vloženie záznamu
            using var context = InMemoryDbContextFactory.Create();

            var origRole = new Role
            {
                RoleName = "ToDeleteRole"
            };
            await context.Roles.AddAsync(origRole);
            await context.SaveChangesAsync();

            // Overíme, že záznam existuje
            var before = await context.Roles.FindAsync(origRole.Id);
            Assert.NotNull(before);

            // ACT: vymažeme ho a uložíme
            context.Roles.Remove(origRole);
            await context.SaveChangesAsync();

            // ASSERT: už by tam nemal byť
            var deleted = await context.Roles.FindAsync(origRole.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task CanAddAndRetrieveUserWithRole()
        {
            // ARRANGE: nový in-memory kontext
            using var context = InMemoryDbContextFactory.Create();

            // Najprv seedneme rolu
            var role = new Role { RoleName = "Manager" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            // Overíme, že zatiaľ nie sú žiadni Users
            Assert.Empty(await context.Users.ToListAsync());

            // Vytvoríme nového Usera s priradenou rolou
            var newUser = new User
            {
                UserName = "testuser",
                PasswordHash = "hashedPw",
                RoleId = role.Id,   // prispojenie k existujúcej role
                Role = role
            };

            // ACT: vložíme a uložime do databázy
            await context.Users.AddAsync(newUser);
            await context.SaveChangesAsync();

            // ASSERT: Načítame späť a overíme hodnoty aj vzťah na Role
            var savedUser = await context.Users
                                         .Include(u => u.Role)
                                         .FirstOrDefaultAsync(u => u.UserName == "testuser");

            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser!.UserName);
            Assert.Equal("hashedPw", savedUser.PasswordHash);
            Assert.NotNull(savedUser.Role);
            Assert.Equal("Manager", savedUser.Role.RoleName);
            Assert.Equal(role.Id, savedUser.RoleId);
        }

        [Fact]
        public async Task RetrievingNonExistentUserReturnsNull()
        {
            // ARRANGE: nový in-memory kontext
            using var context = InMemoryDbContextFactory.Create();

            // ACT: pokus o načítanie neexistujúceho Usera
            var none = await context.Users.FindAsync(Guid.NewGuid());

            // ASSERT: očakávame null
            Assert.Null(none);
        }

        [Fact]
        public async Task CanUpdateUser()
        {
            // ARRANGE: nový in-memory kontext a seed Role + User
            using var context = InMemoryDbContextFactory.Create();

            var role = new Role { RoleName = "Analyst" };
            await context.Roles.AddAsync(role);

            var origUser = new User
            {
                UserName = "olduser",
                PasswordHash = "oldhash",
                RoleId = role.Id,
                Role = role
            };
            await context.Users.AddAsync(origUser);
            await context.SaveChangesAsync();

            // ACT: načítame rovnakého Usera, upravíme a uložime
            var toUpdate = await context.Users.FirstAsync(u => u.UserName == "olduser");
            toUpdate.UserName = "newuser";
            toUpdate.PasswordHash = "newhash";
            await context.SaveChangesAsync();

            // ASSERT: Prečítame ho znovu a skontrolujeme zmenu
            var updated = await context.Users.FindAsync(origUser.Id);
            Assert.NotNull(updated);
            Assert.Equal("newuser", updated!.UserName);
            Assert.Equal("newhash", updated.PasswordHash);
        }

        [Fact]
        public async Task CanDeleteUser()
        {
            // ARRANGE: nový in-memory kontext a seed Role + User
            using var context = InMemoryDbContextFactory.Create();

            var role = new Role { RoleName = "Guest" };
            await context.Roles.AddAsync(role);

            var origUser = new User
            {
                UserName = "deleteuser",
                PasswordHash = "pw",
                RoleId = role.Id,
                Role = role
            };
            await context.Users.AddAsync(origUser);
            await context.SaveChangesAsync();

            // Overíme, že User existuje
            var before = await context.Users.FindAsync(origUser.Id);
            Assert.NotNull(before);

            // ACT: vymažeme ho a uložíme
            context.Users.Remove(origUser);
            await context.SaveChangesAsync();

            // ASSERT: už by tam nemal byť
            var deleted = await context.Users.FindAsync(origUser.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task CanAddUserWithDetails()
        {
            // ARRANGE: nový in-memory kontext a seed Role + User + UserDetails
            using var context = InMemoryDbContextFactory.Create();

            var role = new Role { RoleName = "Reviewer" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var user = new User
            {
                UserName = "detailuser",
                PasswordHash = "pw123",
                RoleId = role.Id,
                Role = role
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Vytvoríme UserDetails pre toho Usera
            var details = new UserDetails
            {
                UserId = user.Id,
                User = user,
                Email = "u@example.com",
                Name = "John",
                Surname = "Doe",
                SignatureImagePath = "sign.png"
            };

            // ACT: vložíme a uložime do databázy
            await context.UserDetails.AddAsync(details);
            await context.SaveChangesAsync();

            // ASSERT: Načítame späť User vrátane UserDetails a overíme vzťah
            var savedUser = await context.Users
                                         .Include(u => u.Details)
                                         .FirstOrDefaultAsync(u => u.UserName == "detailuser");

            Assert.NotNull(savedUser);
            Assert.NotNull(savedUser!.Details);
            Assert.Equal("u@example.com", savedUser.Details.Email);
            Assert.Equal("John", savedUser.Details.Name);
            Assert.Equal("Doe", savedUser.Details.Surname);
            Assert.Equal("sign.png", savedUser.Details.SignatureImagePath);
            Assert.Equal(user.Id, savedUser.Details.UserId);
        }

        [Fact]
        public async Task CanUpdateUserDetails()
        {
            // ARRANGE: nový in-memory kontext, seed Role + User + UserDetails
            using var context = InMemoryDbContextFactory.Create();

            var role = new Role { RoleName = "Editor" };
            await context.Roles.AddAsync(role);

            var user = new User
            {
                UserName = "upuser",
                PasswordHash = "hashup",
                RoleId = role.Id,
                Role = role
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var details = new UserDetails
            {
                UserId = user.Id,
                User = user,
                Email = "old@example.com",
                Name = "OldName",
                Surname = "OldSurname"
            };
            await context.UserDetails.AddAsync(details);
            await context.SaveChangesAsync();

            // ACT: načítame detaily, upravíme a uložime
            var toUpdate = await context.UserDetails.FirstAsync(d => d.UserId == user.Id);
            toUpdate.Email = "new@example.com";
            toUpdate.Name = "NewName";
            toUpdate.Surname = "NewSurname";
            await context.SaveChangesAsync();

            // ASSERT: Prečítame znovu a skontrolujeme zmenu
            var updated = await context.UserDetails.FirstAsync(d => d.UserId == user.Id);
            Assert.NotNull(updated);
            Assert.Equal("new@example.com", updated.Email);
            Assert.Equal("NewName", updated.Name);
            Assert.Equal("NewSurname", updated.Surname);
        }

        [Fact]
        public async Task CanDeleteUserDetails()
        {
            // ARRANGE: nový in-memory kontext, seed Role + User + UserDetails
            using var context = InMemoryDbContextFactory.Create();

            // 1) Seed Role
            var role = new Role { RoleName = "Tester" };
            await context.Roles.AddAsync(role);

            // 2) Seed User (priradíme rolu)
            var user = new User
            {
                UserName = "deluser",
                PasswordHash = "pw",
                RoleId = role.Id,
                Role = role
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // 3) Seed UserDetails so všetkými požadovanými vlastnosťami
            var details = new UserDetails
            {
                UserId = user.Id,
                User = user,
                Email = "del@example.com",
                Name = "John",         // nastavené, lebo v DB je NOT NULL
                Surname = "Doe",       // nastavené, lebo v DB je NOT NULL
                SignatureImagePath = null // toto môže byť null ak to nemáte v DB nastavené ako NOT NULL
            };
            await context.UserDetails.AddAsync(details);
            await context.SaveChangesAsync();

            // Overíme, že UserDetails existuje
            var before = await context.UserDetails.FirstOrDefaultAsync(d => d.UserId == user.Id);
            Assert.NotNull(before);

            // ACT: vymažeme ho a uložíme
            context.UserDetails.Remove(before!);
            await context.SaveChangesAsync();

            // ASSERT: už by tam nemal byť
            var deleted = await context.UserDetails.FirstOrDefaultAsync(d => d.UserId == user.Id);
            Assert.Null(deleted);
        }
    }
}
