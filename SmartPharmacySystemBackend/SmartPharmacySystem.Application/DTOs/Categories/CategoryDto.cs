namespace SmartPharmacySystem.Application.DTOs.Categories;

/// <summary>
/// كائن نقل البيانات للفئة.
/// يُستخدم هذا الكلاس لنقل بيانات الفئة بين الطبقات.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
public class UpdateCategoryDto
{
    public int Id { get; set; }  // غيرت من id صغيرة إلى Id كبيرة
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}