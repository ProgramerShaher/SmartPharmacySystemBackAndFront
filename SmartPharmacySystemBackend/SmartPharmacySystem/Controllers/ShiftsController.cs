using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Shifts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet("Current")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> GetCurrentShift()
    {
        var result = await _shiftService.GetCurrentShiftAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("All")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShiftDto>>>> GetAllShifts()
    {
        var result = await _shiftService.GetAllShiftsAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("Open")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> OpenShift(OpenShiftDto request)
    {
        var result = await _shiftService.OpenShiftAsync(request);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("Close")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> CloseShift(CloseShiftDto request)
    {
        var result = await _shiftService.CloseShiftAsync(request);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}/Summary")]
    public async Task<ActionResult<ApiResponse<ShiftSummaryDto>>> GetShiftSummary(int id)
    {
        var result = await _shiftService.GetShiftSummaryAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}/Details")]
    public async Task<ActionResult<ApiResponse<ShiftDetailsDto>>> GetShiftDetails(int id)
    {
        var result = await _shiftService.GetShiftDetailsAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
