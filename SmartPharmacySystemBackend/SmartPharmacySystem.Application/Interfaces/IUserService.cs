using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.User;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task UpdateUserAsync(int id, UpdateUserDto dto);
        Task DeleteUserAsync(int id);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<PagedResult<UserDto>> SearchAsync(UserQueryDto query);
    }
}
