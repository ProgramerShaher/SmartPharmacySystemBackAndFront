using AutoMapper;
using SmartPharmacySystem.Application.DTOs.DailyClosings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace SmartPharmacySystem.Application.Services;

public class DailyClosingService : IDailyClosingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IShiftService _shiftService;
    private readonly IFinancialService _financialService;
    private readonly SmartPharmacySystem.Application.Interfaces.Data.IApplicationDbContext _context;

    public DailyClosingService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICurrentUserService currentUserService,
        IShiftService shiftService,
        IFinancialService financialService,
        SmartPharmacySystem.Application.Interfaces.Data.IApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _shiftService = shiftService;
        _financialService = financialService;
        _context = context;
    }

    public async Task<DailyClosingDto> GetByIdAsync(int id)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");
        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task<DailyClosingDto?> GetByBranchDateAsync(int branchId, DateTime date)
    {
        var closing = await _unitOfWork.DailyClosings.GetByBranchDateAsync(branchId, date);
        return closing != null ? _mapper.Map<DailyClosingDto>(closing) : null;
    }

    public async Task<IEnumerable<DailyClosingDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetByBranchIdAsync(branchId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<IEnumerable<DailyClosingDto>> GetByDateAsync(DateTime date)
    {
        var closings = await _unitOfWork.DailyClosings.GetByDateAsync(date);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<IEnumerable<DailyClosingDto>> GetPendingApprovalAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetPendingApprovalAsync(dateFrom, dateTo);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<DailyClosingDto> CreateAsync(CreateDailyClosingDto dto)
    {
        var currentBranchId = _currentUserService.GetCurrentBranchId();
        if (!currentBranchId.HasValue)
            throw new InvalidOperationException("لا يمكن إنشاء إغلاق يومي بدون تحديد الفرع الحالي للمستخدم.");

        // ── منع التنفيذ المزدوج (Idempotent) ──
        if (await _unitOfWork.DailyClosings.ExistsAsync(currentBranchId.Value, dto.ClosingDate))
            throw new InvalidOperationException("تم إنشاء إغلاق يومي لهذا التاريخ بالفعل");

        // ── 1. جلب حسابات الدرج والخزينة الرئيسية ──
        var drawerAccount = await _unitOfWork.Financials.GetDrawerAccountAsync();
        var mainSafeAccount = await _unitOfWork.Financials.GetMainSafeAccountAsync();

        // ── 2. حساب رصيد الدرج الفعلي الحالي ──
        var actualCash = await _unitOfWork.Financials.CalculateBalanceAsync(drawerAccount.Id);
        // رصيد افتتاحي اليوم = رصيد الدرج الحالي (قبل الترحيل)
        var openingCash = actualCash;

        // ── 3. حساب مكوّنات اليوم من الحركات ──
        var date = dto.ClosingDate.Date;
        var nextDate = date.AddDays(1);

        var sales = await _context.SaleInvoices
            .Where(s => s.BranchId == currentBranchId.Value
                     && s.CreatedAt >= date && s.CreatedAt < nextDate
                     && (s.Status == DocumentStatus.Approved || s.Status == DocumentStatus.Closed))
            .ToListAsync();

        var returns = await _context.SalesReturns
            .Include(r => r.SaleInvoice)
            .Where(r => r.BranchId == currentBranchId.Value
                     && r.CreatedAt >= date && r.CreatedAt < nextDate
                     && (r.Status == DocumentStatus.Approved || r.Status == DocumentStatus.Closed))
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.BranchId == currentBranchId.Value
                     && e.ExpenseDate >= date && e.ExpenseDate < nextDate)
            .ToListAsync();

        var customerReceipts = await _context.CustomerReceipts
            .Where(c => c.BranchId == currentBranchId.Value
                     && c.ReceiptDate >= date && c.ReceiptDate < nextDate)
            .ToListAsync();

        // ── تصحيح: استخدام PaymentType.Cash بدلاً من (int) == 1 ──
        var totalCashSales = sales
            .Where(s => s.PaymentMethod == PaymentType.Cash)
            .Sum(s => s.TotalAmount);

        var totalCreditSales = sales
            .Where(s => s.PaymentMethod == PaymentType.Credit)
            .Sum(s => s.TotalAmount);

        var totalCardSales = sales
            .Where(s => s.PaymentMethod == PaymentType.BankTransfer)
            .Sum(s => s.TotalAmount);

        var totalCashReturns = returns
            .Where(r => r.SaleInvoice != null && r.SaleInvoice.PaymentMethod == PaymentType.Cash)
            .Sum(r => r.TotalAmount);

        var totalExpenses = expenses.Sum(e => e.Amount);

        var totalCollections = customerReceipts.Sum(c => c.Amount);

        // الرصيد المتوقع والفارق
        var expectedCash = openingCash + totalCashSales + totalCollections - totalCashReturns - totalExpenses;
        var cashVariance = actualCash - expectedCash; // دائماً = 0 في الحالة المثالية

        // ── 4. إنشاء كيان الإغلاق اليومي ──
        var closing = new DailyClosing
        {
            BranchId = currentBranchId.Value,
            ClosingDate = dto.ClosingDate,
            OpeningCash = openingCash,
            ActualCash = actualCash,
            TotalCashSales = totalCashSales,
            TotalCreditSales = totalCreditSales,
            TotalCardSales = totalCardSales,
            TotalCollections = totalCollections,
            TotalCashReturns = totalCashReturns,
            TotalExpenses = totalExpenses,
            // ── اعتماد تلقائي بدون PendingApproval ──
            Status = ClosingStatus.Approved,
            SubmittedByUserId = _currentUserService.UserId ?? 0,
            ApprovedByUserId = _currentUserService.UserId,
            ApprovedAt = DateTime.UtcNow
        };

        await _unitOfWork.DailyClosings.AddAsync(closing);
        await _unitOfWork.SaveChangesAsync();

        // ── 5. تحويل نقدي حقيقي: الدرج → الخزينة الرئيسية ──
        if (actualCash > 0)
        {
            // خصم من الدرج
            await _financialService.ProcessTransactionAsync(
                accountId: drawerAccount.Id,
                amount: actualCash,
                type: FinancialTransactionType.Expense,
                referenceType: ReferenceType.DailyClosing,
                referenceId: closing.Id,
                description: $"إغلاق يومي {dto.ClosingDate:yyyy-MM-dd} — تحويل رصيد الدرج إلى الخزينة الرئيسية");

            // إضافة للخزينة الرئيسية
            await _financialService.ProcessTransactionAsync(
                accountId: mainSafeAccount.Id,
                amount: actualCash,
                type: FinancialTransactionType.Income,
                referenceType: ReferenceType.DailyClosing,
                referenceId: closing.Id,
                description: $"إغلاق يومي {dto.ClosingDate:yyyy-MM-dd} — استلام رصيد الدرج في الخزينة الرئيسية");
        }

        // ── 6. تعليم جميع ورديات الفرع كـ "تم الترحيل" ──
        var pendingShifts = await _context.UserShifts
            .Where(s => s.BranchId == currentBranchId.Value && !s.IsCashTransferredToMainSafe)
            .ToListAsync();

        foreach (var shift in pendingShifts)
        {
            shift.IsCashTransferredToMainSafe = true;
            shift.CashTransferredAt = DateTime.UtcNow;
            shift.TransferredCashAmount = actualCash;
            shift.TransferReferenceNumber = $"DC-{closing.Id}";
        }

        await _context.SaveChangesAsync();

        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task UpdateAsync(DailyClosingDto dto)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");

        _mapper.Map(dto, closing);
        await _unitOfWork.DailyClosings.UpdateAsync(closing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DailyClosingDto> ApproveAsync(int id, int approvedByUserId)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");

        if (closing.Status != ClosingStatus.PendingApproval)
            throw new InvalidOperationException("يمكن فقط اعتماد الإغلاقات المعلقة");

        closing.Status = ClosingStatus.Approved;
        closing.ApprovedByUserId = approvedByUserId;
        closing.ApprovedAt = DateTime.UtcNow;

        await _unitOfWork.DailyClosings.UpdateAsync(closing);
        // ملاحظة: لا نستدعي SweepUntransferredShiftsToMainSafeAsync هنا — 
        // الترحيل المالي الحقيقي يتم في CreateAsync عند الإغلاق الأول
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DailyClosingDto>(closing);
    }

    public async Task DeleteAsync(int id)
    {
        var closing = await _unitOfWork.DailyClosings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الإغلاق اليومي غير موجود");
        await _unitOfWork.DailyClosings.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<DailyClosingDto>> GetApprovedAsync(DateTime dateFrom, DateTime dateTo, int? branchId = null)
    {
        var closings = await _unitOfWork.DailyClosings.GetApprovedAsync(dateFrom, dateTo, branchId);
        return _mapper.Map<IEnumerable<DailyClosingDto>>(closings);
    }

    public async Task<bool> ExistsAsync(int branchId, DateTime date)
    {
        return await _unitOfWork.DailyClosings.ExistsAsync(branchId, date);
    }
}
