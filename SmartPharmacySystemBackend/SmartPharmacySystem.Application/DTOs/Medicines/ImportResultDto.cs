namespace SmartPharmacySystem.Application.DTOs.Medicine;

public class ImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
