namespace SmartPharmacySystem.Application.Interfaces;

public interface IInvoiceNumberGenerator
{
    /// <summary>
    /// Generates a new invoice number for a sale invoice.
    /// Format: SI-YYYY-######
    /// </summary>
    Task<string> GenerateSaleInvoiceNumberAsync();

    /// <summary>
    /// Generates a new invoice number for a purchase invoice.
    /// Format: PI-YYYY-######
    /// </summary>
    Task<string> GeneratePurchaseInvoiceNumberAsync();
}
