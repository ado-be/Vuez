using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <- PRIDAJ

namespace vuez.Models
{
    public class ProgramItemDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DetailId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public string Ppnumber { get; set; } = null!;

        [Required]
        public string Ppname { get; set; } = null!;

        public string? InitialVersionNumber { get; set; }

        public string? DevelopmentTools { get; set; }

        public string? DevelopmentPc { get; set; }

        public string? Connections { get; set; }

        public string? RelatedDocumentation { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public string? Notes { get; set; }

        // ✅ DÔLEŽITÉ: Validáciu tejto navigačnej property vypneme
        [ForeignKey("ItemId")]
        [ValidateNever]
        public virtual ProgramItem Item { get; set; } = null!;

        public virtual ICollection<DistributionList> DistributionLists { get; set; } = new List<DistributionList>();
        public virtual ICollection<PlannedTest> PlannedTests { get; set; } = new List<PlannedTest>();
        public virtual ICollection<ProgramReview> ProgramReviews { get; set; } = new List<ProgramReview>();
        public virtual ICollection<ProgramVerification> ProgramVerifications { get; set; } = new List<ProgramVerification>();
        public virtual ICollection<RepeatedTest> RepeatedTests { get; set; } = new List<RepeatedTest>();
    }
}
