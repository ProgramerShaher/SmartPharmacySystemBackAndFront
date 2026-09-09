using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل ملف نمط وإعدادات النشاط التجاري (Business Profile)
/// وهو مصدر الحقيقة الوحيد الذي تتكيف بناءً عليه واجهات وقواعد أعمال النظام
/// </summary>
public class BusinessProfile : BaseEntity
{
    [Required]
    public BusinessType BusinessType { get; set; } = BusinessType.Pharmacy;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = "صيدلية";

    /// <summary>
    /// هل هذا الملف هو النشط والمعتمد حالياً للنظام؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// هل تم تخصيص هذا الملف يدوياً من قبل المستخدم؟
    /// </summary>
    public bool IsCustom { get; set; } = false;

    // ─── إعدادات بطاقة الصنف (Item Card Settings) ─────────────────────────

    /// <summary>
    /// تتبع تاريخ الصلاحية وإلزاميته
    /// </summary>
    public bool TrackExpiryDate { get; set; } = true;

    /// <summary>
    /// تتبع رقم التشغيلة/الدفعة
    /// </summary>
    public bool TrackBatchNumber { get; set; } = true;

    /// <summary>
    /// تتبع الأرقام التسلسلية الفردية (Serial Numbers / IMEI)
    /// </summary>
    public bool TrackSerialNumbers { get; set; } = false;

    /// <summary>
    /// استخدام مصفوفة المقاسات والألوان (Product Variants)
    /// </summary>
    public bool UseProductVariants { get; set; } = false;

    /// <summary>
    /// السماح بالكميات العشرية والكسور (كجم، متر، لتر)
    /// </summary>
    public bool AllowDecimalQuantity { get; set; } = false;

    /// <summary>
    /// دعم قراءة وتوليد باركود الميزان الإلكتروني
    /// </summary>
    public bool UseScaleBarcode { get; set; } = false;

    /// <summary>
    /// دعم تسجيل وتتبع فترات الضمان
    /// </summary>
    public bool HasWarranty { get; set; } = false;

    /// <summary>
    /// دعم البحث عن البدائل (البدائل الدوائية بالصيدلية أو بدائل القطع)
    /// </summary>
    public bool HasAlternatives { get; set; } = true;

    /// <summary>
    /// تتبع أبعاد وحجم المنتج (طول × عرض × ارتفاع)
    /// </summary>
    public bool TrackDimensions { get; set; } = false;

    /// <summary>
    /// تتبع رقم الموديل ورقم القطعة المصنعي
    /// </summary>
    public bool TrackModelNumber { get; set; } = false;

    /// <summary>
    /// إلزامية الباركود عند إضافة الصنف
    /// </summary>
    public bool RequireBarcode { get; set; } = true;

    /// <summary>
    /// إلزامية الفئة والتصنيف عند إضافة الصنف
    /// </summary>
    public bool RequireCategory { get; set; } = true;

    // ─── إعدادات وسياسات المبيعات ونقاط البيع (Sales & POS) ────────────────

    /// <summary>
    /// إلزامية تحديد عميل في فاتورة المبيعات (false يسمح بالزبون النقدي/الطيار)
    /// </summary>
    public bool RequireCustomer { get; set; } = false;

    /// <summary>
    /// السماح بالبيع بسعر أقل من سعر التكلفة
    /// </summary>
    public bool AllowSellBelowCost { get; set; } = false;

    /// <summary>
    /// إلزامية فتح وردية كاشير لتنفيذ عمليات البيع
    /// </summary>
    public bool RequireShiftToSell { get; set; } = true;

    /// <summary>
    /// فحص سقف ائتمان العميل عند البيع الآجل
    /// </summary>
    public bool CheckCustomerCreditLimit { get; set; } = true;

    /// <summary>
    /// منع البيع الآجل تماماً بدون اختيار عميل مسجل
    /// </summary>
    public bool PreventCreditSaleWithoutCustomer { get; set; } = true;

    /// <summary>
    /// صرف وبيع الدفعات الأقرب انتهاءً أولاً (First Expired First Out)
    /// </summary>
    public bool UseFEFO { get; set; } = true;

    /// <summary>
    /// دعم الدفع المتعدد والمجزأ في الفاتورة الواحدة (كاش + شبكة)
    /// </summary>
    public bool AllowMultiPayment { get; set; } = true;

    /// <summary>
    /// إمكانية تعليق واسترجاع الفاتورة (Hold Invoice) في الكاشير
    /// </summary>
    public bool AllowHoldInvoice { get; set; } = true;

    /// <summary>
    /// فتح درج النقود آلياً عند الطباعة أو السداد
    /// </summary>
    public bool UseCashDrawer { get; set; } = true;

    /// <summary>
    /// قالب طباعة الفاتورة الافتراضي
    /// </summary>
    public InvoicePrintTemplate DefaultPrintTemplate { get; set; } = InvoicePrintTemplate.Thermal80mm;

    // ─── إعدادات المشتريات (Purchases) ────────────────────────────────────

    /// <summary>
    /// إلزامية أمر الشراء (PO) قبل إدخال فاتورة المشتريات
    /// </summary>
    public bool RequirePurchaseOrder { get; set; } = false;

    /// <summary>
    /// احتساب وتوزيع مصاريف الشحن والجمارك على تكلفة الأصناف (Landed Cost)
    /// </summary>
    public bool UseLandedCost { get; set; } = false;

    // ─── إعدادات الضريبة (VAT) ───────────────────────────────────────────

    /// <summary>
    /// تفعيل ضريبة القيمة المضافة في الفواتير
    /// </summary>
    public bool EnableVAT { get; set; } = false;

    /// <summary>
    /// النسبة المئوية الافتراضية للضريبة
    /// </summary>
    public decimal DefaultVATRate { get; set; } = 0m;
}
