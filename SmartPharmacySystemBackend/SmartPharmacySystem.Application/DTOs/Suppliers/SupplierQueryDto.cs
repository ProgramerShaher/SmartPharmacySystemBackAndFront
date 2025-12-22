using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.Suppliers;

public class SupplierQueryDto : BaseQueryDto
{
    public bool? HasBalance { get; set; }
}
