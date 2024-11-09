using System.ComponentModel.DataAnnotations;

public class Indicators
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}