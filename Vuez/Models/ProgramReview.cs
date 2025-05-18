using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class ProgramReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ak je ReviewId auto-increment
        public int ReviewId { get; set; }

        [Required]
        public int DetailId { get; set; }

        public string? ReviewForm { get; set; }

        public string? ReviewResult { get; set; }

        public string? Reviewer { get; set; }

        public string? ReviewerSignature { get; set; }

        // Ak EF Core nepozná DateOnly, nahraď DateTime?
        public DateOnly? ReviewDate { get; set; }

        // Navigácia
        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }
}
