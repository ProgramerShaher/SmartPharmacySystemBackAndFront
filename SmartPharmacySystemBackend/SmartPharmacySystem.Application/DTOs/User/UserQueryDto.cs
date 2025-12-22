using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.User;

public class UserQueryDto : BaseQueryDto
{
    public string? Role { get; set; }
    public bool? IsDeleted { get; set; }
}
