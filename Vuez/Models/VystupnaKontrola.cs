using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vuez.Models
{
    [Table("VystupnaKontrola")] // Explicitné mapovanie na SQL tabuľku
    public class VystupnaKontrola
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? CisloProtokolu { get; set; }

        [Required]
        [StringLength(510)]
        public string NazovVyrobku { get; set; }

        [Required]
        [StringLength(510)]
        public string Objednavatel { get; set; }

        [Required]
        [StringLength(100)]
        public string ZakazkoveCislo { get; set; }

        [StringLength(510)]
        public string? KontrolaPodla { get; set; }

        [StringLength(510)]
        public string? KompletnostTechaVyr { get; set; }

        [StringLength(510)]
        public string? KompletnostKontrolnych { get; set; }

        [StringLength(510)]
        public string? KompletnostSprievodnej { get; set; }

        [StringLength(510)]
        public string? Pripravenostkexp { get; set; }

        [StringLength(int.MaxValue)]
        public string? Poznamky { get; set; }

        [StringLength(510)]
        public string? Miesto { get; set; }

        public DateTime? Datum { get; set; }

        // 🔶 Pridané podpisové polia
      

        [StringLength(1000)]
        public string? PodpisManagerUrl { get; set; }

        [StringLength(1000)]
        public string? PodpisTechnikUrl { get; set; }
    }
}
