using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class PlannedTest
    {
        [Key] // Primárny kľúč
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment pre TestId
        public int TestId { get; set; }

        [Required]
        public int DetailId { get; set; }

        [Required]
        public string TestName { get; set; } = null!;

        // DateOnly je podporované v EF Core 6+, ak máš nižšiu verziu, použi DateTime
        public DateOnly? PlannedDate { get; set; }

        public DateOnly? ExecutionDate { get; set; }

        public string? TestResult { get; set; }

        public string? Notes { get; set; }

        // Navigácia k ProgramItemDetail
        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }
}
