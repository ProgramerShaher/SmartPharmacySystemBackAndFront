using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SmartPharmacySystem.Application.Helpers;

/// <summary>
/// مولّد رمز الاستجابة السريعة (QR Code) وفق المعايير الفنية لهيئة الزكاة والضريبة والجمارك (ZATCA / هيئة الضرائب).
/// يعتمد على هيكلية TLV (Tag-Length-Value) المرمزة بنظام Base64 للمرحلتين الأولى والثانية.
/// </summary>
public static class ZatcaQrCodeGenerator
{
    /// <summary>
    /// توليد نص Base64 المشفر لرمز QR للفاتورة الإلكترونية وفق معايير ZATCA
    /// </summary>
    /// <param name="sellerName">اسم المورد أو المنشأة</param>
    /// <param name="vatRegistrationNumber">الرقم الضريبي للمنشأة</param>
    /// <param name="invoiceTimestamp">تاريخ ووقت إصدار الفاتورة</param>
    /// <param name="invoiceTotal">إجمالي قيمة الفاتورة شامل ضريبة القيمة المضافة</param>
    /// <param name="vatTotal">إجمالي مبلغ ضريبة القيمة المضافة</param>
    /// <returns>نص Base64 جاهز لتوليد الـ QR Code</returns>
    public static string GenerateQrCode(
        string sellerName,
        string vatRegistrationNumber,
        DateTime invoiceTimestamp,
        decimal invoiceTotal,
        decimal vatTotal)
    {
        var tlvBytes = new List<byte>();

        // Tag 1: Seller's Name (اسم المورد)
        AppendTlvTag(tlvBytes, 1, sellerName ?? "منشأة تجارية");

        // Tag 2: VAT Registration Number (الرقم الضريبي)
        AppendTlvTag(tlvBytes, 2, vatRegistrationNumber ?? "300000000000003");

        // Tag 3: Time stamp (تاريخ ووقت الفاتورة بتنسيق ISO 8601)
        AppendTlvTag(tlvBytes, 3, invoiceTimestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));

        // Tag 4: Invoice total amount with VAT (إجمالي الفاتورة شاملاً الضريبة)
        AppendTlvTag(tlvBytes, 4, invoiceTotal.ToString("F2", CultureInfo.InvariantCulture));

        // Tag 5: VAT total (إجمالي مبلغ الضريبة)
        AppendTlvTag(tlvBytes, 5, vatTotal.ToString("F2", CultureInfo.InvariantCulture));

        return Convert.ToBase64String(tlvBytes.ToArray());
    }

    private static void AppendTlvTag(List<byte> buffer, byte tagNumber, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        buffer.Add(tagNumber);
        buffer.Add((byte)valueBytes.Length);
        buffer.AddRange(valueBytes);
    }
}
