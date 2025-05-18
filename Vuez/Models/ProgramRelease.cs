using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class ProgramRelease
    {
        [Key]
        public int ReleaseId { get; set; }

        [Required]
        public int DetailId { get; set; }

        public string? Documentation { get; set; }

        public string? ReleasedBy { get; set; }

        public bool IsReleased { get; set; }

        public string? ReleaseSignature { get; set; }

        public DateTime? ReleasedDate { get; set; }

        [ForeignKey("DetailId")]
        public virtual ProgramItemDetail Detail { get; set; } = null!;
    }

}
