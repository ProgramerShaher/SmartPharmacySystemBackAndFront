using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
