using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class RepeatedTest
    {
        [Key] // 👈 Primárny kľúč
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestId { get; set; }

        [Required]
        public int DetailId { get; set; }

        [Required]
        public string TestName { get; set; } = null!;

        // Ak EF Core verzia nepodporuje DateOnly, nahraď DateTime?
        public DateOnly? PlannedDate { get; set; }

        public DateOnly? ExecutionDate { get; set; }

        public string? TestResult { get; set; }

        public string? Notes { get; set; }

        // Navigačná vlastnosť
        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }
}
