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
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJournalEntryService _journalEntryService;
    private readonly SmartPharmacySystem.Core.Interfaces.IUnitOfWork _unitOfWork;

    public ShiftService(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        IJournalEntryService journalEntryService,
        SmartPharmacySystem.Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _journalEntryService = journalEntryService;
        _unitOfWork = unitOfWork;
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
        
        // Ensure Drawer Account Exists
        var user = await _context.Users.FindAsync(userId.Value);
        var drawerCode = $"11101-{userId.Value}";
        var drawerAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == drawerCode);
        if (drawerAccount == null)
        {
            var mainSafe = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "11101") ?? await _context.Accounts.FirstOrDefaultAsync();
            drawerAccount = new Account
            {
                Code = drawerCode,
                Name = $"درج الكاشير - {user?.FullName ?? user?.Username}",
                Type = SmartPharmacySystem.Core.Enums.AccountType.Asset,
                ParentId = mainSafe?.Id,
                IsMainAccount = false,
                IsActive = true
            };
            _context.Accounts.Add(drawerAccount);
        }

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

        // Close all Approved Invoices in this shift
        var shiftInvoices = await _context.SaleInvoices
            .Where(i => i.UserShiftId == shift.Id && i.Status == Core.Enums.DocumentStatus.Approved)
            .ToListAsync();
        
        foreach (var inv in shiftInvoices)
        {
            inv.Status = Core.Enums.DocumentStatus.Closed;
        }

        await _context.SaveChangesAsync();

        // ----------------------------------------------------
        // إنشاء قيد يومية آلي لترحيل النقدية للصندوق الرئيسي
        // ----------------------------------------------------
        if (request.TransferToMainSafe && request.ActualClosingCash > 0)
        {
            var drawerCode = $"11101-{userId.Value}";
            var drawerAccount = await _unitOfWork.Accounts.GetByCodeAsync(drawerCode);
            var mainSafe = await _unitOfWork.Accounts.GetByCodeAsync("11101") ?? await _unitOfWork.Accounts.GetByCodeAsync("11100");

            if (drawerAccount != null && mainSafe != null)
            {
                var journalEntry = new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryDto
                {
                    EntryDate = DateTime.UtcNow,
                    VoucherNumber = $"SH-{shift.Id}",
                    Description = $"ترحيل نقدية إغلاق وردية رقم {shift.Id} - كاشير: {shift.User?.FullName ?? shift.User?.Username}",
                    Type = SmartPharmacySystem.Core.Enums.VoucherType.JournalEntry,
                    Lines = new List<SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto>
                    {
                        new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                        {
                            AccountId = mainSafe.Id,
                            Debit = request.ActualClosingCash,
                            Credit = 0,
                            Description = "استلام نقدية الوردية"
                        },
                        new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                        {
                            AccountId = drawerAccount.Id,
                            Debit = 0,
                            Credit = request.ActualClosingCash,
                            Description = "ترحيل نقدية الوردية"
                        }
                    }
                };
                
                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId.Value);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId.Value);
            }
        }

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
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && (s.Status == Core.Enums.DocumentStatus.Approved || s.Status == Core.Enums.DocumentStatus.Closed))
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
            .Where(r => r.CreatedBy == userId && r.CreatedAt >= startTime && r.CreatedAt <= endTime && (r.Status == Core.Enums.DocumentStatus.Approved || r.Status == Core.Enums.DocumentStatus.Closed))
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
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && (s.Status == Core.Enums.DocumentStatus.Approved || s.Status == Core.Enums.DocumentStatus.Closed))
            .ToListAsync();

        var salesReturns = await _context.SalesReturns
            .Include(s => s.SaleInvoice)
            .Where(s => s.CreatedBy == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime && (s.Status == Core.Enums.DocumentStatus.Approved || s.Status == Core.Enums.DocumentStatus.Closed))
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

        var expectedCash = shift.OpeningCash + totalSalesCash + totalCustomerReceiptsCash - totalReturnsCash - totalExpensesCash;

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
