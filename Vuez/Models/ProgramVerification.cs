using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class ProgramVerification
    {
        [Key] // ✅ Primárny kľúč
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int ReviewId { get; set; }

        [Required]
        public int DetailId { get; set; }

        public string? ReviewForm { get; set; }
        public string? ReviewResult { get; set; }
        public string? Reviewer { get; set; }
        public string? ReviewerSignature { get; set; }

        // ⚠️ Pozor na DateOnly – funguje až od EF Core 6+ s .NET 6+
        public DateOnly? ReviewDate { get; set; }

        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }
}
