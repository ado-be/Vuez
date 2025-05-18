using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    [Table("VstupnaKontrola")] // Explicitné mapovanie na SQL tabuľku
    public class VstupnaKontrola
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Primárny kľúč
         public int? CisloProtokolu { get; set; }

        [Required]
        [StringLength(510)]
        public string NazovVyrobku { get; set; }

        [Required]
        [StringLength(510)]
        public string Dodavatel { get; set; }

        [Required]
        [StringLength(100)]
        public string ZakazkoveCislo { get; set; }

        [StringLength(int.MaxValue)] 
        public string? KontrolaPodla { get; set; }

        [StringLength(int.MaxValue)]
        public string? SpravnostDodavky { get; set; }

        [StringLength(int.MaxValue)]
        public string? ZnacenieMaterialu { get; set; }

        [StringLength(int.MaxValue)]
        public string? CistotaPovrchu { get; set; }

        [StringLength(int.MaxValue)]
        public string? Balenie { get; set; }

        [StringLength(int.MaxValue)]
        public string? Poskodenie { get; set; }

        [StringLength(int.MaxValue)]
        public string? IneKroky { get; set; }

        [StringLength(int.MaxValue)]
        public string? Poznamky { get; set; }

        [StringLength(1000)]
        public string? SuborCesta { get; set; }

        [StringLength(int.MaxValue)]
        public string? Miesto {  get; set; }

        public DateTime? Datum { get; set; }

       
        public string? PodpisManagerUrl { get; set; }
        public string? PodpisTechnikUrl { get; set; }


    }
}
