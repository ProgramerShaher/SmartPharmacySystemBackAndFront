using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Helpers;

/// <summary>
/// Helper class to generate localized alert messages based on alert type and batch information.
/// فئة مساعدة لإنشاء رسائل التنبيه المترجمة بناءً على نوع التنبيه ومعلومات الدفعة.
/// </summary>
public static class AlertMessageHelper
{
    /// <summary>
    /// Generates a localized alert message based on alert type and batch information.
    /// ينشئ رسالة تنبيه مترجمة بناءً على نوع التنبيه ومعلومات الدفعة.
    /// </summary>
    /// <param name="alertType">Type of the alert</param>
    /// <param name="batch">Medicine batch information</param>
    /// <param name="language">Language code (default: "ar" for Arabic)</param>
    /// <returns>Localized alert message</returns>
    public static string GenerateMessage(AlertType alertType, MedicineBatch? batch, string language = "ar")
    {
        if (batch == null)
            return language == "ar" ? "تنبيه: معلومات الدفعة غير متوفرة" : "Alert: Batch information unavailable";

        var medicineName = batch.Medicine?.Name ?? "غير محدد";
        var batchNumber = batch.CompanyBatchNumber;
        var expiryDate = batch.ExpiryDate.ToString("yyyy-MM-dd");

        return language == "ar"
            ? GenerateArabicMessage(alertType, medicineName, batchNumber, expiryDate)
            : GenerateEnglishMessage(alertType, medicineName, batchNumber, expiryDate);
    }

    private static string GenerateArabicMessage(AlertType alertType, string medicineName, string batchNumber, string expiryDate)
    {
        return alertType switch
        {
            AlertType.ExpiryOneWeek => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName} ستنتهي خلال أسبوع واحد (تاريخ الانتهاء: {expiryDate})",
            AlertType.ExpiryTwoWeeks => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName} ستنتهي خلال أسبوعين (تاريخ الانتهاء: {expiryDate})",
            AlertType.ExpiryOneMonth => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName} ستنتهي خلال شهر واحد (تاريخ الانتهاء: {expiryDate})",
            AlertType.ExpiryTwoMonths => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName} ستنتهي خلال شهرين (تاريخ الانتهاء: {expiryDate})",
            AlertType.Expired => $"تحذير: الدفعة {batchNumber} للمادة {medicineName} منتهية الصلاحية (تاريخ الانتهاء: {expiryDate})",
            AlertType.LowStock => $"تنبيه: مخزون منخفض للدفعة {batchNumber} للمادة {medicineName}",
            AlertType.Damaged => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName} تالفة",
            _ => $"تنبيه: الدفعة {batchNumber} للمادة {medicineName}"
        };
    }

    private static string GenerateEnglishMessage(AlertType alertType, string medicineName, string batchNumber, string expiryDate)
    {
        return alertType switch
        {
            AlertType.ExpiryOneWeek => $"Alert: Batch {batchNumber} of {medicineName} will expire within 1 week (Expiry: {expiryDate})",
            AlertType.ExpiryTwoWeeks => $"Alert: Batch {batchNumber} of {medicineName} will expire within 2 weeks (Expiry: {expiryDate})",
            AlertType.ExpiryOneMonth => $"Alert: Batch {batchNumber} of {medicineName} will expire within 1 month (Expiry: {expiryDate})",
            AlertType.ExpiryTwoMonths => $"Alert: Batch {batchNumber} of {medicineName} will expire within 2 months (Expiry: {expiryDate})",
            AlertType.Expired => $"Warning: Batch {batchNumber} of {medicineName} has expired (Expiry: {expiryDate})",
            AlertType.LowStock => $"Alert: Low stock for batch {batchNumber} of {medicineName}",
            AlertType.Damaged => $"Alert: Batch {batchNumber} of {medicineName} is damaged",
            _ => $"Alert: Batch {batchNumber} of {medicineName}"
        };
    }

    /// <summary>
    /// Gets the message key for localization resources.
    /// يحصل على مفتاح الرسالة لموارد الترجمة.
    /// </summary>
    /// <param name="alertType">Type of the alert</param>
    /// <returns>Message key for localization</returns>
    public static string GetMessageKey(AlertType alertType)
    {
        return alertType switch
        {
            AlertType.ExpiryOneWeek => "Alert.Expiry.OneWeek",
            AlertType.ExpiryTwoWeeks => "Alert.Expiry.TwoWeeks",
            AlertType.ExpiryOneMonth => "Alert.Expiry.OneMonth",
            AlertType.ExpiryTwoMonths => "Alert.Expiry.TwoMonths",
            AlertType.Expired => "Alert.Expired",
            AlertType.LowStock => "Alert.LowStock",
            AlertType.Damaged => "Alert.Damaged",
            _ => "Alert.Unknown"
        };
    }
}
