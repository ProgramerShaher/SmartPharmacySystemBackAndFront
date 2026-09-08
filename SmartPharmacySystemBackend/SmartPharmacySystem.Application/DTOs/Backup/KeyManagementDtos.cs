using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.Backup;

public class ExportKeyDto
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة للتأكيد.")]
    public string CurrentPassword { get; set; } = string.Empty;
}

public class ExportKeyResponseDto
{
    public string Base64Key { get; set; } = string.Empty;
}

public class ImportKeyDto
{
    [Required(ErrorMessage = "مفتاح الاسترداد مطلوب.")]
    public string Base64Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة للتأكيد.")]
    public string CurrentPassword { get; set; } = string.Empty;
}
