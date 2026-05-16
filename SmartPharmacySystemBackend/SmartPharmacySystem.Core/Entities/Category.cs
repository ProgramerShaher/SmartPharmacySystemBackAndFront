namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a category for medicines.
/// Categories help organize medicines for better management.
/// </summary>
public class Category : BaseEntity
{

    /// <summary>
    /// Name of the category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL of the category image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Collection of medicines belonging to this category.
    /// </summary>
    public ICollection<Medicine> Medicines { get; set; }
}