namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// نوع الدفع - نقدي أو آجل
/// Payment type - Cash or Credit
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// دفع نقدي فوري
    /// Immediate cash payment
    /// </summary>
    Cash = 1,

    /// <summary>
    /// دفع آجل (على الحساب)
    /// Credit payment (deferred)
    /// </summary>
    Credit = 2
}
