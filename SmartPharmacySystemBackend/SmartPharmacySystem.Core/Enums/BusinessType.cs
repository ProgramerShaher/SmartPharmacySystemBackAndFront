namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// يحدد أنواع الأنشطة التجارية المدعومة في النظام
/// </summary>
public enum BusinessType
{
    /// <summary>
    /// صيدلية (تتبع صلاحية، تشغيلة، FEFO، بدائل دوائية)
    /// </summary>
    Pharmacy = 1,

    /// <summary>
    /// سوبرماركت وهايبرماركت (صلاحية، باركود ميزان، زبون طيار، تعليق فاتورة)
    /// </summary>
    Supermarket = 2,

    /// <summary>
    /// بقالة ومحلات تجزئة غذائية
    /// </summary>
    Grocery = 3,

    /// <summary>
    /// ملابس وأحذية وأكسسوارات (مقاسات، ألوان، مواسم)
    /// </summary>
    Fashion = 4,

    /// <summary>
    /// أجهزة وإلكترونيات وهواتف (سيريال، IMEI، ضمان)
    /// </summary>
    Electronics = 5,

    /// <summary>
    /// مواد بناء وسيراميك وحديد (كميات عشرية، أبعاد، مصاريف شحن)
    /// </summary>
    BuildingMaterials = 6,

    /// <summary>
    /// قطع غيار سيارات ومعدات (رقم قطعة، سيارات متوافقة)
    /// </summary>
    AutoParts = 7,

    /// <summary>
    /// تجارة جملة وتوزيع (أوامر شراء، سقف ائتمان، مندوبين)
    /// </summary>
    Wholesale = 8,

    /// <summary>
    /// أثاث ومفروشات (أبعاد، عنوان شحن، ضمان)
    /// </summary>
    Furniture = 9,

    /// <summary>
    /// تجارة عامة (بدون قيود تخصصية)
    /// </summary>
    GeneralTrade = 10
}
