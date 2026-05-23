using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class Branch : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string BranchCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Location { get; set; }

    [Required]
    public BranchType BranchType { get; set; } = BranchType.Sub;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
