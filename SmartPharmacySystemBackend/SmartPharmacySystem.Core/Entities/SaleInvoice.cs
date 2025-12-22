using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a sale invoice in the pharmacy system.
/// Sale invoices record medicine sales to customers.
/// </summary>
public class SaleInvoice
{
    /// <summary>
    /// Unique identifier for the sale invoice.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique number for the sale invoice (Format: SI-YYYY-######).
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of the sale.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Total amount of the sale.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Total cost of the medicines sold.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Total profit from the sale.
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string PaymentMethod { get; set; }

    /// <summary>
    /// Name of the customer (optional).
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Date and time when the invoice was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID of the user who created this invoice.
    /// معرف المستخدم الذي أنشأ هذه الفاتورة.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Status of the document (Draft, Approved, Cancelled).
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;

    /// <summary>
    /// Collection of sale invoice details.
    /// </summary>
    public ICollection<SaleInvoiceDetail> SaleInvoiceDetails { get; set; }

    /// <summary>
    /// Collection of sales returns related to this invoice.
    /// </summary>
    public ICollection<SalesReturn> SalesReturns { get; set; }
}