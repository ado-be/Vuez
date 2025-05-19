using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    public class ConfigurationSheet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigId { get; set; }

        [Required(ErrorMessage = "Pole 'Názov APV' je povinné.")]
        public string Apvname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pole 'Číslo APV' je povinné.")]
        public string Apvnumber { get; set; } = string.Empty;

        public string? ContractNumber { get; set; }

        [Required(ErrorMessage = "Pole 'Zákazkové číslo' je povinné.")]
        public string OrderNumber { get; set; } = string.Empty;

        public string? Processor { get; set; }

        public string? RelatedHwsw { get; set; }

        public string? RelatedDocumentation { get; set; }

        public DateTime? CreatedDate { get; set; }

        // Navigácia
        [ValidateNever]
        public virtual ICollection<ProgramItem> ProgramItems { get; set; } = new List<ProgramItem>();
    }
}
