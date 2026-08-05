using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.DTOs.Shifts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ShiftService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<ShiftDto>> GetCurrentShiftAsync()
    {
        var userId = _currentUserService.UserId;
        var branchId = _currentUserService.GetCurrentBranchId();

        if (userId == null || branchId == null)
            return ApiResponse<ShiftDto>.Failed("User or Branch not found in context.");

        var shift = await _context.UserShifts
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.BranchId == branchId && s.Status == "Open")
            .FirstOrDefaultAsync();

        if (shift == null)
            return ApiResponse<ShiftDto>.Failed("No open shift found.");

        return ApiResponse<ShiftDto>.Succeeded(MapToDto(shift), "Success");
    }

    public async Task<ApiResponse<IEnumerable<ShiftDto>>> GetAllShiftsAsync()
    {
        var branchId = _currentUserService.GetCurrentBranchId();
        if (branchId == null)
            return ApiResponse<IEnumerable<ShiftDto>>.Failed("Branch not found in context.");

        var shifts = await _context.UserShifts
            .Include(s => s.User)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var shiftDtos = shifts.Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<ShiftDto>>.Succeeded(shiftDtos, "Success");
    }

    public async Task<ApiResponse<ShiftDto>> OpenShiftAsync(OpenShiftDto request)
    {
        var userId = _currentUserService.UserId;
        var branchId = _currentUserService.GetCurrentBranchId();

        if (userId == null || branchId == null)
            return ApiResponse<ShiftDto>.Failed("User or Branch not found in context.");

        // Check if there is already an open shift
        var existingShift = await _context.UserShifts
            .Where(s => s.UserId == userId && s.BranchId == branchId && s.Status == "Open")
            .FirstOrDefaultAsync();

        if (existingShift != null)
            return ApiResponse<ShiftDto>.Failed("You already have an open shift.");

        var shift = new UserShift
        {
            UserId = userId.Value,
            BranchId = branchId.Value,
            StartTime = DateTime.UtcNow,
            OpeningCash = request.OpeningCash,
            ExpectedClosingCash = request.OpeningCash, // Will be updated by sales/returns logic later if needed
            Status = "Open",
            Notes = request.Notes
        };

        _context.UserShifts.Add(shift);
        await _context.SaveChangesAsync();

        // Reload to get navigation properties
        var savedShift = await _context.UserShifts
            .Include(s => s.User)
            .FirstAsync(s => s.Id == shift.Id);

        return ApiResponse<ShiftDto>.Succeeded(MapToDto(savedShift), "Success");
    }

    public async Task<ApiResponse<ShiftDto>> CloseShiftAsync(CloseShiftDto request)
    {
        var userId = _currentUserService.UserId;
        var branchId = _currentUserService.GetCurrentBranchId();

        if (userId == null || branchId == null)
            return ApiResponse<ShiftDto>.Failed("User or Branch not found in context.");

        var shift = await _context.UserShifts
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.BranchId == branchId && s.Status == "Open")
            .FirstOrDefaultAsync();

        if (shift == null)
            return ApiResponse<ShiftDto>.Failed("No open shift found to close.");

        // Here we could calculate ExpectedClosingCash dynamically based on Sales, Returns, Expenses during the shift
        // For now, we will assume it's simply OpeningCash + (sales - returns - expenses) which would be updated
        // dynamically or computed here.
        // As a simplified example, we'll just compute difference against the current ExpectedClosingCash.

        shift.EndTime = DateTime.UtcNow;
        shift.ActualClosingCash = request.ActualClosingCash;
        shift.Difference = request.ActualClosingCash - shift.ExpectedClosingCash;
        shift.Status = "Closed";

        if (!string.IsNullOrEmpty(request.Notes))
        {
            shift.Notes = string.IsNullOrEmpty(shift.Notes) ? request.Notes : $"{shift.Notes}\n{request.Notes}";
        }

        await _context.SaveChangesAsync();

        return ApiResponse<ShiftDto>.Succeeded(MapToDto(shift), "Success");
    }

    private ShiftDto MapToDto(UserShift shift)
    {
        return new ShiftDto
        {
            Id = shift.Id,
            UserId = shift.UserId,
            UserName = shift.User?.FullName,
            BranchId = shift.BranchId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            OpeningCash = shift.OpeningCash,
            ActualClosingCash = shift.ActualClosingCash,
            ExpectedClosingCash = shift.ExpectedClosingCash,
            Difference = shift.Difference,
            Status = shift.Status,
            Notes = shift.Notes
        };
    }
}
