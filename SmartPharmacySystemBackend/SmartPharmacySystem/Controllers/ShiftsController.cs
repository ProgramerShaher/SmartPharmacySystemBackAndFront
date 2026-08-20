using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPharmacySystem.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Shifts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly IAccountService _accountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ShiftsController(
        IShiftService shiftService, 
        IAccountService accountService, 
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _shiftService = shiftService;
        _accountService = accountService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
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

    [RequirePermission("sales.shifts.manage")]
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

    [RequirePermission("sales.shifts.manage")]
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

    [HttpGet("my-drawer-ledger")]
    public async Task<IActionResult> GetMyDrawerLedger([FromQuery] System.DateTime? startDate, [FromQuery] System.DateTime? endDate)
    {
        var userId = _currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var shiftResult = await _shiftService.GetCurrentShiftAsync();
        if (!shiftResult.Success || shiftResult.Data == null)
        {
            return BadRequest(ApiResponse<string>.Failed("لم يتم العثور على وردية مفتوحة."));
        }
        var shift = shiftResult.Data;

        var code = $"11101-{userId}";
        var account = await _unitOfWork.Accounts.GetByCodeAsync(code);
        
        if (account == null)
        {
            return BadRequest(ApiResponse<string>.Failed("لم يتم العثور على درج خاص بك. يرجى فتح وردية أولاً."));
        }

        // Adding a 1-hour backward buffer to account for stale timestamps if the frontend page was left open before saving
        var start = startDate ?? shift.StartTime.AddHours(-1);
        var end = endDate ?? System.DateTime.UtcNow.AddDays(1);

        var ledger = await _accountService.GetGeneralLedgerAsync(account.Id, start, end);
        return Ok(ApiResponse<SmartPharmacySystem.Application.DTOs.Financial.LedgerReportDto>.Succeeded(ledger, "تم الجلب بنجاح"));
    }

    [HttpGet("debug-drawer")]
    public async Task<IActionResult> DebugDrawer()
    {
        var userId = _currentUserService.UserId;
        var code = $"11101-{userId}";
        var account = await _unitOfWork.Accounts.GetByCodeAsync(code);
        if (account == null) return Ok("No account found");

        var lines = await _unitOfWork.JournalEntries.GetLinesByAccountIdAsync(account.Id, null, null);
        var result = lines.Select(l => new {
            l.Id,
            l.Debit,
            l.Credit,
            EntryDate = l.JournalEntry.EntryDate,
            IsDeleted = l.JournalEntry.IsDeleted,
            l.JournalEntry.VoucherNumber
        }).ToList();

        return Ok(new {
            AccountId = account.Id,
            Code = account.Code,
            Lines = result
        });
    }
}
