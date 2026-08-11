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

        // Calculate ExpectedClosingCash dynamically
        var summary = await CalculateShiftSummaryAsync(shift);
        
        shift.ExpectedClosingCash = summary.ExpectedClosingCash;
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

    public async Task<ApiResponse<ShiftSummaryDto>> GetShiftSummaryAsync(int shiftId)
    {
        var shift = await _context.UserShifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == shiftId);

        if (shift == null)
            return ApiResponse<ShiftSummaryDto>.Failed("Shift not found.");

        var summary = await CalculateShiftSummaryAsync(shift);
        return ApiResponse<ShiftSummaryDto>.Succeeded(summary, "Success");
    }

    public async Task<ApiResponse<ShiftDetailsDto>> GetShiftDetailsAsync(int shiftId)
    {
        var shift = await _context.UserShifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == shiftId);

        if (shift == null)
            return ApiResponse<ShiftDetailsDto>.Failed("Shift not found.");

        var summary = await CalculateShiftSummaryAsync(shift);

        var endTime = shift.EndTime ?? DateTime.UtcNow;
        var startTime = shift.StartTime;
        var userId = shift.UserId;

        var sales = await _context.SaleInvoices
            .Include(s => s.Customer)
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && s.Status == Core.Enums.DocumentStatus.Approved)
            .Select(s => new ShiftInvoiceDto
            {
                Id = s.Id,
                InvoiceNumber = s.SaleInvoiceNumber ?? s.Id.ToString(),
                CustomerName = s.Customer != null ? s.Customer.Name : (s.CustomerName ?? "نقدي"),
                PaymentMethod = s.PaymentMethod.ToString(),
                TotalAmount = s.TotalAmount,
                CreatedAt = s.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync();

        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.CreatedBy == userId && e.ExpenseDate >= startTime && e.ExpenseDate <= endTime)
            .Select(e => new ShiftExpenseDto
            {
                Id = e.Id,
                ReferenceNumber = e.Id.ToString(),
                CategoryName = e.Category != null ? e.Category.Name : "غير محدد",
                Amount = e.Amount,
                Notes = e.Notes ?? string.Empty,
                CreatedAt = e.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync();

        var returns = await _context.SalesReturns
            .Include(r => r.Customer)
            .Where(r => r.CreatedBy == userId && r.CreatedAt >= startTime && r.CreatedAt <= endTime && r.Status == Core.Enums.DocumentStatus.Approved)
            .Select(r => new ShiftReturnDto
            {
                Id = r.Id,
                CustomerName = r.Customer != null ? r.Customer.Name : "غير محدد",
                TotalAmount = r.TotalAmount,
                Reason = r.Reason ?? string.Empty,
                CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync();

        var details = new ShiftDetailsDto
        {
            Summary = summary,
            Invoices = sales,
            Expenses = expenses,
            Returns = returns
        };

        return ApiResponse<ShiftDetailsDto>.Succeeded(details, "Success");
    }

    private async Task<ShiftSummaryDto> CalculateShiftSummaryAsync(UserShift shift)
    {
        var endTime = shift.EndTime ?? DateTime.UtcNow;
        var startTime = shift.StartTime;
        var userId = shift.UserId;

        // Sales (Cash = 1, Visa = 2, Credit = 3 assuming Core.Enums.PaymentMethod)
        var sales = await _context.SaleInvoices
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && s.Status == Core.Enums.DocumentStatus.Approved)
            .ToListAsync();

        var salesReturns = await _context.SalesReturns
            .Include(s => s.SaleInvoice)
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && s.Status == Core.Enums.DocumentStatus.Approved)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.CreatedBy == userId && e.ExpenseDate >= startTime && e.ExpenseDate <= endTime)
            .ToListAsync();

        var customerReceipts = await _context.CustomerReceipts
            .Where(c => c.CreatedBy == userId && c.ReceiptDate >= startTime && c.ReceiptDate <= endTime)
            .ToListAsync();

        var supplierPayments = await _context.SupplierPayments
            .Where(s => s.CreatedBy == userId && s.PaymentDate >= startTime && s.PaymentDate <= endTime)
            .ToListAsync();

        var totalSalesCash = sales.Where(s => (int)s.PaymentMethod == 1).Sum(s => s.TotalAmount); // Cash
        var totalSalesNetwork = sales.Where(s => (int)s.PaymentMethod == 2).Sum(s => s.TotalAmount); // Visa/Mada
        var totalSalesCredit = sales.Where(s => (int)s.PaymentMethod == 3).Sum(s => s.TotalAmount); // Credit

        var totalReturnsCash = salesReturns.Where(s => s.SaleInvoice != null && (int)s.SaleInvoice.PaymentMethod == 1).Sum(s => s.TotalAmount);

        // Assume expenses are cash if not specified differently, or paid from drawer
        var totalExpensesCash = expenses.Sum(e => e.Amount);

        var totalCustomerReceiptsCash = customerReceipts.Sum(c => c.Amount);
        var totalSupplierPaymentsCash = supplierPayments.Sum(s => s.Amount);

        var expectedCash = shift.OpeningCash + totalSalesCash + totalCustomerReceiptsCash - totalReturnsCash - totalExpensesCash - totalSupplierPaymentsCash;

        return new ShiftSummaryDto
        {
            ShiftId = shift.Id,
            UserName = shift.User?.FullName ?? string.Empty,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            Status = shift.Status,
            Notes = shift.Notes ?? string.Empty,
            OpeningCash = shift.OpeningCash,
            TotalSalesCash = totalSalesCash,
            TotalSalesNetwork = totalSalesNetwork,
            TotalSalesCredit = totalSalesCredit,
            InvoicesCount = sales.Count,
            TotalReturnsCash = totalReturnsCash,
            TotalCustomerReceiptsCash = totalCustomerReceiptsCash,
            TotalSupplierPaymentsCash = totalSupplierPaymentsCash,
            TotalExpensesCash = totalExpensesCash,
            ExpectedClosingCash = expectedCash,
            ActualClosingCash = shift.ActualClosingCash,
            Difference = shift.Difference
        };
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
