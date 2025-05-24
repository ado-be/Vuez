using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using vuez.Models;

[Table("ProgramRelease")]
public class ProgramRelease
{
    [Key]
    [Column("ReleaseID")]
    public int ReleaseId { get; set; }

    [Required]
    [Column("DetailID")]
    public int DetailId { get; set; }

    [Column("Documentation")]
    public string? Documentation { get; set; }

    [Column("ReleasedBy")]
    public string? ReleasedBy { get; set; }

    [Column("IsReleased")]
    public bool IsReleased { get; set; }

    [Column("ReleaseSignature")]
    public string? ReleaseSignature { get; set; }

    // ✅ TOTO MUSÍ BYŤ Guid? (NIE int?)
    [Column("ReleasedByUserId")]
    public Guid? ReleasedByUserId { get; set; }

    [Column("ReleasedDate")]
    public DateTime? ReleasedDate { get; set; }

    [ForeignKey("DetailId")]
    public virtual ProgramItemDetail Detail { get; set; } = null!;

    [ForeignKey("ReleasedByUserId")]
    public virtual User? ReleasedByUser { get; set; }
}