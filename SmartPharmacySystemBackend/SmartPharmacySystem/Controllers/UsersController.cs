using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.User;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IAuthService authService,
            ICurrentUserService currentUserService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _authService = authService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Search and paginate users with optional filters
        /// </summary>
        /// <access>Admin</access>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryDto query)
        {
            var result = await _userService.SearchAsync(query);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<UserDto>>.Succeeded(result, "No users found matching the search criteria"));

            return Ok(ApiResponse<PagedResult<UserDto>>.Succeeded(result, "Users retrieved successfully"));
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <access>Admin</access>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid user ID provided"));

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<object>.Failed($"User with ID {id} not found", 404));

            return Ok(ApiResponse<UserDto>.Succeeded(user, "User retrieved successfully"));
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <access>Admin</access>
        [HttpPost]
        //[AllowAnonymous] // لا تنسى فتحها مؤقتاً
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات غير صالحة"));

            // تشفير كلمة السر ووضعها في الحقل الموحد PasswordHash
            dto.PasswordHash = _authService.HashPassword(dto.PasswordHash);
            dto.ConfirmPassword = dto.PasswordHash;

            // الآن المابينج سيعمل بسلاسة لأن الأسماء متطابقة (PasswordHash -> PasswordHash)
            var createdUser = await _userService.CreateUserAsync(dto);

            return StatusCode(201, ApiResponse<UserDto>.Succeeded(createdUser, "User created successfully", 201));
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <access>Admin</access>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("User ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid user data provided"));

            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"User with ID {id} not found", 404));

            // If password is provided, hash it
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var hashedPassword = _authService.HashPassword(dto.Password);
                dto.Password = hashedPassword;
                dto.ConfirmPassword = hashedPassword;
            }

            await _userService.UpdateUserAsync(id, dto);
            var updatedUser = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDto>.Succeeded(updatedUser, "User updated successfully"));
        }

        /// <summary>
        /// Delete a user (soft delete)
        /// </summary>
        /// <access>Admin</access>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid user ID provided"));

            // Prevent deleting yourself
            if (_currentUserService.UserId == id)
                return BadRequest(ApiResponse<object>.Failed("لا يمكنك حذف حسابك الخاص"));

            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"User with ID {id} not found", 404));

            await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "User deleted successfully"));
        }
    }
}
