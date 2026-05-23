using AutoMapper;
using SmartPharmacySystem.Application.DTOs.MonthlySalaries;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class MonthlySalaryService : IMonthlySalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MonthlySalaryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
