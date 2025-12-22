namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a sequence for invoice numbering.
/// Tracks the last used number for a specific type and year.
/// </summary>
public class InvoiceNumberSequence
{
    /// <summary>
    /// Primary key for the sequence.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of invoice (e.g., "SI" for Sale Invoice, "PI" for Purchase Invoice).
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Year of the sequence.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Last used sequence number.
    /// </summary>
    public int LastNumber { get; set; }
}
