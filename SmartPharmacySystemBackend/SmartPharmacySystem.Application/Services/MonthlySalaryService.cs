using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.DTOs.MonthlySalaries;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class MonthlySalaryService : IMonthlySalaryService
{
    private const int DefaultPaidFromAccountId = 1101;
    private const int DefaultSalaryExpenseAccountId = 5201;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJournalEntryService _journalEntryService;
    private readonly IFinancialService _financialService;

    public MonthlySalaryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IJournalEntryService journalEntryService,
        IFinancialService financialService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _journalEntryService = journalEntryService;
        _financialService = financialService;
    }

    public async Task<MonthlySalaryDto> GetByIdAsync(int id)
    {
        var salary = await _unitOfWork.MonthlySalaries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الراتب غير موجود");
        return MapToDto(salary);
    }

    public async Task<MonthlySalaryDto?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year)
    {
        var salary = await _unitOfWork.MonthlySalaries.GetByEmployeeMonthYearAsync(employeeId, month, year);
        return salary != null ? MapToDto(salary) : null;
    }

    public async Task<IEnumerable<MonthlySalaryDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var salaries = await _unitOfWork.MonthlySalaries.GetByEmployeeIdAsync(employeeId);
        return salaries.Select(MapToDto);
    }

    public async Task<IEnumerable<MonthlySalaryDto>> GetByBranchMonthYearAsync(int branchId, int month, int year)
    {
        var salaries = await _unitOfWork.MonthlySalaries.GetByBranchMonthYearAsync(branchId, month, year);
        return salaries.Select(MapToDto);
    }

    public async Task<IEnumerable<MonthlySalaryDto>> GetByMonthYearAsync(int month, int year, int? branchId = null)
    {
        var salaries = await _unitOfWork.MonthlySalaries.GetByMonthYearAsync(month, year, branchId);
        return salaries.Select(MapToDto);
    }

    public async Task<MonthlySalaryDto> CreateAsync(CreateMonthlySalaryDto dto)
    {
        if (await _unitOfWork.MonthlySalaries.ExistsAsync(dto.EmployeeId, dto.Month, dto.Year))
            throw new InvalidOperationException("راتب هذا الشهر موجود بالفعل لهذا الموظف");

        var salary = _mapper.Map<MonthlySalary>(dto);
        await _unitOfWork.MonthlySalaries.AddAsync(salary);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(salary);
    }

    public async Task UpdateAsync(UpdateMonthlySalaryDto dto)
    {
        var salary = await _unitOfWork.MonthlySalaries.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("الراتب غير موجود");

        salary.BasicSalary = dto.BasicSalary;
        salary.TotalAllowances = dto.TotalAllowances;
        salary.TotalBonuses = dto.TotalBonuses;
        salary.TotalDeductions = dto.TotalDeductions;

        await _unitOfWork.MonthlySalaries.UpdateAsync(salary);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var salary = await _unitOfWork.MonthlySalaries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الراتب غير موجود");
        await _unitOfWork.MonthlySalaries.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalPayrollAsync(int month, int year, int? branchId = null)
    {
        return await _unitOfWork.MonthlySalaries.GetTotalPayrollAsync(month, year, branchId);
    }

    public async Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year, int? branchId = null)
    {
        var salaries = (await _unitOfWork.MonthlySalaries.GetByMonthYearAsync(month, year, branchId)).ToList();

        return new PayrollSummaryDto
        {
            TotalEmployees = salaries.Count(),
            PaidCount = salaries.Count(s => s.PaymentStatus == PaymentStatus.Paid),
            PendingCount = salaries.Count(s => s.PaymentStatus == PaymentStatus.Pending),
            TotalBasicSalary = salaries.Sum(s => s.BasicSalary),
            TotalAllowances = salaries.Sum(s => s.TotalAllowances),
            TotalBonuses = salaries.Sum(s => s.TotalBonuses),
            TotalDeductions = salaries.Sum(s => s.TotalDeductions),
            TotalNetSalary = salaries.Sum(s => s.NetSalary)
        };
    }

    public async Task<bool> ExistsAsync(int employeeId, int month, int year)
    {
        return await _unitOfWork.MonthlySalaries.ExistsAsync(employeeId, month, year);
    }

    public async Task<MonthlySalaryDto> PaySalaryAsync(int id, PaySalaryDto dto, int? userId = null)
    {
        var salary = await _unitOfWork.MonthlySalaries.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الراتب غير موجود");

        if (salary.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("تم صرف هذا الراتب مسبقاً");

        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            await PaySalaryInternalAsync(salary, dto, userId);
            });
            return MapToDto(salary);
        }
        catch
        {
            throw;
        }
    }

    public async Task<int> PayAllAsync(int month, int year, int? branchId, PaySalaryDto dto, int? userId = null)
    {
        var salaries = (await _unitOfWork.MonthlySalaries.GetByMonthYearAsync(month, year, branchId))
            .Where(s => s.PaymentStatus == PaymentStatus.Pending)
            .ToList();

        try
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
            foreach (var salary in salaries)
            {
                await PaySalaryInternalAsync(salary, dto, userId, saveImmediately: false);
            }

            await _unitOfWork.SaveChangesAsync();
            });
            return salaries.Count;
        }
        catch
        {
            throw;
        }
    }

    private async Task PaySalaryInternalAsync(MonthlySalary salary, PaySalaryDto dto, int? userId, bool saveImmediately = true)
    {
        var paidFromAccountId = dto.PaidFromAccountId > 0 ? dto.PaidFromAccountId : DefaultPaidFromAccountId;
        var salaryExpenseAccountId = dto.SalaryExpenseAccountId ?? DefaultSalaryExpenseAccountId;

        var paidFromAccount = await _unitOfWork.Accounts.GetByIdAsync(paidFromAccountId)
            ?? throw new KeyNotFoundException("حساب صرف الراتب غير موجود");

        if (paidFromAccount.IsMainAccount || !paidFromAccount.IsActive)
            throw new InvalidOperationException("يجب اختيار حساب صرف فرعي ونشط");

        var salaryExpenseAccount = await _unitOfWork.Accounts.GetByIdAsync(salaryExpenseAccountId)
            ?? await _unitOfWork.Accounts.GetByCodeAsync("52001")
            ?? await _unitOfWork.Accounts.GetByCodeAsync("5201")
            ?? throw new KeyNotFoundException("حساب مصروف الرواتب غير موجود");

        if (salaryExpenseAccount.IsMainAccount || !salaryExpenseAccount.IsActive)
            throw new InvalidOperationException("يجب اختيار حساب مصروف رواتب فرعي ونشط");

        var amount = salary.NetSalary;
        if (amount <= 0)
            throw new InvalidOperationException("صافي الراتب يجب أن يكون أكبر من صفر قبل الصرف");

        var employeeName = salary.Employee?.FullName ?? $"Employee #{salary.EmployeeId}";
        var period = $"{salary.Month:D2}/{salary.Year}";

        var journalEntry = new JournalEntryDto
        {
            EntryDate = DateTime.UtcNow,
            VoucherNumber = $"SAL-{salary.Id}",
            Description = $"صرف راتب {employeeName} عن شهر {period}",
            Type = VoucherType.PaymentVoucher,
            Lines = new List<JournalEntryLineDto>
            {
                new()
                {
                    AccountId = salaryExpenseAccount.Id,
                    Debit = amount,
                    Credit = 0,
                    Description = $"إثبات مصروف راتب {employeeName}"
                },
                new()
                {
                    AccountId = paidFromAccount.Id,
                    Debit = 0,
                    Credit = amount,
                    Description = $"صرف راتب {employeeName} من {paidFromAccount.Name}"
                }
            }
        };

        var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
        await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

        await _financialService.ProcessTransactionAsync(
            accountId: paidFromAccount.Id,
            amount: amount,
            type: FinancialTransactionType.Expense,
            referenceType: ReferenceType.MonthlySalary,
            referenceId: salary.Id,
            description: $"صرف راتب {employeeName} عن شهر {period}");

        salary.PaymentStatus = PaymentStatus.Paid;
        salary.PaidAt = DateTime.UtcNow;
        salary.PaidFromAccountId = paidFromAccount.Id;
        salary.SalaryExpenseAccountId = salaryExpenseAccount.Id;
        salary.JournalEntryId = createdEntry.Id;

        await _unitOfWork.MonthlySalaries.UpdateAsync(salary);

        if (saveImmediately)
            await _unitOfWork.SaveChangesAsync();
    }

    private MonthlySalaryDto MapToDto(MonthlySalary salary)
    {
        var dto = _mapper.Map<MonthlySalaryDto>(salary);
        dto.NetSalary = salary.NetSalary;
        dto.MonthName = GetMonthName(salary.Month);
        dto.PaymentStatusName = GetPaymentStatusName(salary.PaymentStatus);
        dto.PaymentStatusColor = GetPaymentStatusColor(salary.PaymentStatus);
        return dto;
    }

    private static string GetMonthName(int month) => month switch
    {
        1 => "يناير", 2 => "فبراير", 3 => "مارس", 4 => "أبريل",
        5 => "مايو", 6 => "يونيو", 7 => "يوليو", 8 => "أغسطس",
        9 => "سبتمبر", 10 => "أكتوبر", 11 => "نوفمبر", 12 => "ديسمبر",
        _ => month.ToString()
    };

    private static string GetPaymentStatusName(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "قيد الانتظار",
        PaymentStatus.Paid => "مدفوع",
        _ => status.ToString()
    };

    private static string GetPaymentStatusColor(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "warning",
        PaymentStatus.Paid => "success",
        _ => "secondary"
    };
}
