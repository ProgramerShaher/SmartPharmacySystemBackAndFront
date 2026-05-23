namespace SmartPharmacySystem.Application.DTOs.InvoiceSequences;

public class InvoiceSequenceDto
{
    public int Id { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LastNumber { get; set; }
    public string NextNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
