using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using vuez.Models;

[Table("ProgramVerification")]
public class ProgramVerification
{
    [Key]
    [Column("ReviewID")]
    public int VerificationId { get; set; }

    [Required]
    [Column("DetailID")]
    public int DetailId { get; set; }

    [Column("ReviewForm")]
    public string? ReviewForm { get; set; }

    [Column("ReviewResult")]
    public string? ReviewResult { get; set; }

    [Column("Reviewer")]
    public string? Reviewer { get; set; }

    [Column("ReviewerSignature")]
    public string? ReviewerSignature { get; set; }

    // ✅ TOTO MUSÍ BYŤ Guid? (NIE int?)
    [Column("VerifierUserId")]
    public Guid? VerifierUserId { get; set; }

    [Column("ReviewDate")]
    public DateOnly? ReviewDate { get; set; }

    [ForeignKey("DetailId")]
    public virtual ProgramItemDetail Detail { get; set; } = null!;

    [ForeignKey("VerifierUserId")]
    public virtual User? VerifierUser { get; set; }
}