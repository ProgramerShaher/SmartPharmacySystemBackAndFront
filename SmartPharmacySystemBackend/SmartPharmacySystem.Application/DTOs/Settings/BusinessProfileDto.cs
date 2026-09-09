using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Settings;

public class BusinessProfileDto
{
    public int Id { get; set; }
    public BusinessType BusinessType { get; set; }
    public string BusinessTypeName => BusinessType.ToString();
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsCustom { get; set; }

    // ─── إعدادات بطاقة الصنف ─────────────────────────
    public bool TrackExpiryDate { get; set; }
    public bool TrackBatchNumber { get; set; }
    public bool TrackSerialNumbers { get; set; }
    public bool UseProductVariants { get; set; }
    public bool AllowDecimalQuantity { get; set; }
    public bool UseScaleBarcode { get; set; }
    public bool HasWarranty { get; set; }
    public bool HasAlternatives { get; set; }
    public bool TrackDimensions { get; set; }
    public bool TrackModelNumber { get; set; }
    public bool RequireBarcode { get; set; }
    public bool RequireCategory { get; set; }

    // ─── إعدادات وسياسات المبيعات ────────────────────
    public bool RequireCustomer { get; set; }
    public bool AllowSellBelowCost { get; set; }
    public bool RequireShiftToSell { get; set; }
    public bool CheckCustomerCreditLimit { get; set; }
    public bool PreventCreditSaleWithoutCustomer { get; set; }
    public bool UseFEFO { get; set; }
    public bool AllowMultiPayment { get; set; }
    public bool AllowHoldInvoice { get; set; }
    public bool UseCashDrawer { get; set; }
    public InvoicePrintTemplate DefaultPrintTemplate { get; set; }
    public string DefaultPrintTemplateName => DefaultPrintTemplate.ToString();

    // ─── إعدادات المشتريات ───────────────────────────
    public bool RequirePurchaseOrder { get; set; }
    public bool UseLandedCost { get; set; }

    // ─── إعدادات الضريبة ─────────────────────────────
    public bool EnableVAT { get; set; }
    public decimal DefaultVATRate { get; set; }
}

public class UpdateBusinessProfileDto
{
    public string DisplayName { get; set; } = string.Empty;

    // ─── إعدادات بطاقة الصنف ─────────────────────────
    public bool TrackExpiryDate { get; set; }
    public bool TrackBatchNumber { get; set; }
    public bool TrackSerialNumbers { get; set; }
    public bool UseProductVariants { get; set; }
    public bool AllowDecimalQuantity { get; set; }
    public bool UseScaleBarcode { get; set; }
    public bool HasWarranty { get; set; }
    public bool HasAlternatives { get; set; }
    public bool TrackDimensions { get; set; }
    public bool TrackModelNumber { get; set; }
    public bool RequireBarcode { get; set; }
    public bool RequireCategory { get; set; }

    // ─── إعدادات وسياسات المبيعات ────────────────────
    public bool RequireCustomer { get; set; }
    public bool AllowSellBelowCost { get; set; }
    public bool RequireShiftToSell { get; set; }
    public bool CheckCustomerCreditLimit { get; set; }
    public bool PreventCreditSaleWithoutCustomer { get; set; }
    public bool UseFEFO { get; set; }
    public bool AllowMultiPayment { get; set; }
    public bool AllowHoldInvoice { get; set; }
    public bool UseCashDrawer { get; set; }
    public InvoicePrintTemplate DefaultPrintTemplate { get; set; }

    // ─── إعدادات المشتريات ───────────────────────────
    public bool RequirePurchaseOrder { get; set; }
    public bool UseLandedCost { get; set; }

    // ─── إعدادات الضريبة ─────────────────────────────
    public bool EnableVAT { get; set; }
    public decimal DefaultVATRate { get; set; }
}
