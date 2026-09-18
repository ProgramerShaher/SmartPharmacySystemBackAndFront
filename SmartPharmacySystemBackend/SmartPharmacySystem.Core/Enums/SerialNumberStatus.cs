namespace SmartPharmacySystem.Core.Enums
{
    /// <summary>
    /// حالة الرقم التسلسلي للجهاز أو المنتج (إلكترونيات وأجهزة)
    /// </summary>
    public enum SerialNumberStatus
    {
        /// <summary>
        /// متوفر في المخزن
        /// </summary>
        InStock = 1,

        /// <summary>
        /// تم بيعه للعميل
        /// </summary>
        Sold = 2,

        /// <summary>
        /// مرتجع من العميل
        /// </summary>
        Returned = 3,

        /// <summary>
        /// تالف / عطل مصنعي
        /// </summary>
        Defective = 4,

        /// <summary>
        /// تحت الصيانة / الضمان
        /// </summary>
        UnderMaintenance = 5
    }
}
