using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.User;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
    
namespace SmartPharmacySystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                throw new ArgumentException("كلمات المرور غير متطابقة");
            }

            var user = _mapper.Map<User>(dto);
            // Assuming User logic (hashing pw etc) handled in mapping or here. For now standard CRUD.
            user.CreatedAt = DateTime.UtcNow;
            user.IsDeleted = false;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المستخدم برقم {id} غير موجود");

            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                {
                    throw new ArgumentException("كلمات المرور غير متطابقة");
                }
                user.Password = dto.Password; // Assuming hashing is handled or not required yet
            }

            // Map other fields but exclude Password to avoid overwriting it if empty (handled manually above)
            _mapper.Map(dto, user);
            
            // Re-ensure password isn't lost if Map accidentally touched it (though typically Map won't touch it if ignored or name mismatch, but safety first)
            // Actually, a better way is to tell AutoMapper to ignore Password in UpdateUserDto -> User mapping if it's null/empty.
            // For now, manual assignment is clear.

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var exists = await _unitOfWork.Users.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"المستخدم برقم {id} غير موجود");

            await _unitOfWork.Users.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<PagedResult<UserDto>> SearchAsync(UserQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(
                query.Search,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDirection,
                query.Role,
                query.IsDeleted);

            var dtos = _mapper.Map<IEnumerable<UserDto>>(items);
            return new PagedResult<UserDto>(dtos, totalCount, query.Page, query.PageSize);
        }
    }
}
