using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace vuez.Models
{
    public class ProgramItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        [Required]
        public int ConfigId { get; set; }

        [Required(ErrorMessage = "Pole 'Označenie' je povinné.")]
        public string ItemCode { get; set; } = null!;

        [Required(ErrorMessage = "Pole 'Názov' je povinné.")]
        public string ItemName { get; set; } = null!;

        public string? ItemDescription { get; set; }

       
        [ValidateNever]
        public virtual ConfigurationSheet Config { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<ProgramItemDetail> ProgramItemDetails { get; set; } = new List<ProgramItemDetail>();
    }
}
