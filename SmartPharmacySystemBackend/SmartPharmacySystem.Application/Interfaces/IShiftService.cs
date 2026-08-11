using System.Threading.Tasks;
using SmartPharmacySystem.Application.DTOs.Shifts;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IShiftService
{
    Task<ApiResponse<ShiftDto>> GetCurrentShiftAsync();
    Task<ApiResponse<IEnumerable<ShiftDto>>> GetAllShiftsAsync();
    Task<ApiResponse<ShiftDto>> OpenShiftAsync(OpenShiftDto request);
    Task<ApiResponse<ShiftDto>> CloseShiftAsync(CloseShiftDto request);
    Task<ApiResponse<ShiftSummaryDto>> GetShiftSummaryAsync(int shiftId);
    Task<ApiResponse<ShiftDetailsDto>> GetShiftDetailsAsync(int shiftId);
}
