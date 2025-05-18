using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class DistributionList
    {
        [Key] // Primárny kľúč
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DistributionId { get; set; }

        [Required]
        public int DetailId { get; set; }

        public string? CopyNumber { get; set; }

        public string? StorageLocation { get; set; }

        public string? Medium { get; set; }

        public int? Quantity { get; set; }

        public string? StoredBy { get; set; }

        public DateOnly? StorageDate { get; set; }

        public string? Signature { get; set; }

        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }
}
