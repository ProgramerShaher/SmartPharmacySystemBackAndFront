namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// Specifies the type of transaction for barcode processing
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Selling products to customers
    /// </summary>
    Sale = 1,

    /// <summary>
    /// Purchasing products from suppliers
    /// </summary>
    Purchase = 2,

    /// <summary>
    /// Returning products (Sales Return or Purchase Return)
    /// </summary>
    Return = 3
}
