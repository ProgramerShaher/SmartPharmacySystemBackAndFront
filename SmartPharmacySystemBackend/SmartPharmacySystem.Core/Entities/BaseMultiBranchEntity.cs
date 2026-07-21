namespace SmartPharmacySystem.Core.Entities;

public abstract class BaseMultiBranchEntity : BaseEntity
{
    public int? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
}
