using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace vuez.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string UserName { get; set; }
        [Required]
        public string PasswordHash { get; set; }

        // Tieto vlastnosti zostávajú, aby sa zachovala kompatibilita s existujúcim kódom
     
        [Required]
        [ForeignKey("Role")]
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }

        // Navigačná vlastnosť na UserDetails
        public virtual UserDetails Details { get; set; }
    }

    public class Role
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string RoleName { get; set; }
    }

    public class UserDetails
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Väzba na používateľa - jeden používateľ má jeden UserDetails
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Detailné údaje o používateľovi
        public string Email { get; set; }

        // Duplikácia mena a priezviska, ak ich chcete presunúť z User do UserDetails
        public string Name { get; set; }
        public string Surname { get; set; }

        // Cesta k obrázku podpisu (môže byť null)
        public string SignatureImagePath { get; set; }

        // Dátum poslednej aktualizácie
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    
}