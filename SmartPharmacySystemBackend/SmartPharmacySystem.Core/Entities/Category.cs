namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a category for medicines.
/// Categories help organize medicines for better management.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date and time when the category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Collection of medicines belonging to this category.
    /// </summary>
    public ICollection<Medicine> Medicines { get; set; }
}