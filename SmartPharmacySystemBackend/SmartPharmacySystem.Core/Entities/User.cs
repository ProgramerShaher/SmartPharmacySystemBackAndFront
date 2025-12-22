namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a user in the pharmacy management system.
/// Users can perform various operations like sales, purchases, and inventory management.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username for login authentication.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password hash for authentication.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Full name of the user.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Role of the user (e.g., Admin, Pharmacist, Cashier).
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Reset password code/token.
    /// </summary>
    public string? ResetPasswordCode { get; set; }

    /// <summary>
    /// Phone number for contact.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Additional notes about the user.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID of the user who created this record.
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }
}