using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Attendances;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AttendanceDto> GetByIdAsync(int id)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("سجل الحضور غير موجود");
        return _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByEmployeeIdAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var records = await _unitOfWork.Attendances.GetByEmployeeIdAsync(employeeId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<AttendanceDto>>(records);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var records = await _unitOfWork.Attendances.GetByBranchIdAsync(branchId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<AttendanceDto>>(records);
    }

    public async Task<AttendanceDto?> GetTodayAttendanceAsync(int employeeId)
    {
        var attendance = await _unitOfWork.Attendances.GetTodayAttendanceAsync(employeeId);
        return attendance != null ? _mapper.Map<AttendanceDto>(attendance) : null;
    }

    public async Task<AttendanceDto> CheckInAsync(CreateAttendanceDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.EmployeeId)
            ?? throw new KeyNotFoundException("الموظف غير موجود");

        EnsureAttendanceDateAllowed(employee, DateTime.Now);

        var todayAttendance = await _unitOfWork.Attendances.GetTodayAttendanceAsync(dto.EmployeeId);
        if (todayAttendance != null)
            throw new InvalidOperationException("تم تسجيل الحضور بالفعل لهذا اليوم");

        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            CheckIn = DateTime.Now,
            WorkingBranchId = dto.WorkingBranchId,
            Shift = dto.Shift,
            AttendanceStatus = AttendanceStatus.Present
        };

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<AttendanceDto> CheckOutAsync(int attendanceId, DateTime checkOutTime)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId)
            ?? throw new KeyNotFoundException("سجل الحضور غير موجود");

        attendance.CheckOut = checkOutTime;
        await _unitOfWork.Attendances.UpdateAsync(attendance);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task UpdateAsync(UpdateAttendanceDto dto)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("سجل الحضور غير موجود");

        _mapper.Map(dto, attendance);
        await _unitOfWork.Attendances.UpdateAsync(attendance);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("سجل الحضور غير موجود");
        await _unitOfWork.Attendances.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetAbsentCountAsync(int branchId, DateTime date)
    {
        return await _unitOfWork.Attendances.GetAbsentCountAsync(branchId, date);
    }

    public async Task<AttendanceDto> MarkAbsentAsync(int employeeId, DateTime date)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException("الموظف غير موجود");

        EnsureAttendanceDateAllowed(employee, date);

        var existingAttendance = await _unitOfWork.Attendances.GetAttendanceByDateAsync(employeeId, date);
        var wasAlreadyAbsent = existingAttendance?.AttendanceStatus == AttendanceStatus.Absent;
        
        Attendance attendance;
        if (existingAttendance != null)
        {
            attendance = existingAttendance;
            attendance.AttendanceStatus = AttendanceStatus.Absent;
            await _unitOfWork.Attendances.UpdateAsync(attendance);
        }
        else
        {
            attendance = new Attendance
            {
                EmployeeId = employeeId,
                CheckIn = date.Date, // Set to start of the day
                WorkingBranchId = employee.BranchId,
                Shift = ShiftType.Morning, // Default
                AttendanceStatus = AttendanceStatus.Absent
            };
            await _unitOfWork.Attendances.AddAsync(attendance);
        }

        // Penalty Logic: (Basic Salary / 30) for one day of absence
        decimal penaltyAmount = Math.Round(employee.BasicSalary / 30m, 2);
        var deductionDescription = $"خصم غياب يوم {date:yyyy-MM-dd}: الراتب الأساسي {employee.BasicSalary:N2} / 30 = {penaltyAmount:N2}";

        var monthlySalary = await _unitOfWork.MonthlySalaries.GetByEmployeeMonthYearAsync(employeeId, date.Month, date.Year);
        if (wasAlreadyAbsent || monthlySalary?.Deductions.Any(d => d.DeductionType == DeductionType.Absence && d.Description.Contains(date.ToString("yyyy-MM-dd"))) == true)
        {
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AttendanceDto>(attendance);
        }

        if (monthlySalary != null)
        {
            // If Monthly Salary already exists, add deduction directly
            monthlySalary.TotalDeductions += penaltyAmount;
            
            var deductionItem = new SalaryDeductionItem
            {
                MonthlySalaryId = monthlySalary.Id,
                DeductionType = DeductionType.Absence,
                Amount = penaltyAmount,
                Description = deductionDescription
            };
            monthlySalary.Deductions.Add(deductionItem);
            
            // Add deduction item (if repository exists, else just rely on TotalDeductions update)
            await _unitOfWork.MonthlySalaries.UpdateAsync(monthlySalary);
        }
        else
        {
            // If Monthly Salary doesn't exist, create it with the initial deduction
            var newSalary = new MonthlySalary
            {
                EmployeeId = employeeId,
                BranchId = employee.BranchId,
                Month = date.Month,
                Year = date.Year,
                BasicSalary = employee.BasicSalary,
                TotalAllowances = 0,
                TotalBonuses = 0,
                TotalDeductions = penaltyAmount,
                PaymentStatus = PaymentStatus.Pending
            };
            
            // Wait, we can't easily insert SalaryDeductionItem if MonthlySalary is not saved yet to get ID.
            // But EF Core will handle it if we add to the collection.
            newSalary.Deductions.Add(new SalaryDeductionItem
            {
                DeductionType = DeductionType.Absence,
                Amount = penaltyAmount,
                Description = deductionDescription
            });
            
            await _unitOfWork.MonthlySalaries.AddAsync(newSalary);
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<AttendanceDto> MarkPresentAsync(int employeeId, DateTime date)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException("الموظف غير موجود");

        EnsureAttendanceDateAllowed(employee, date);

        var existingAttendance = await _unitOfWork.Attendances.GetAttendanceByDateAsync(employeeId, date);
        var wasAbsent = existingAttendance?.AttendanceStatus == AttendanceStatus.Absent;
        
        Attendance attendance;
        if (existingAttendance != null)
        {
            attendance = existingAttendance;
            attendance.AttendanceStatus = AttendanceStatus.Present;
            await _unitOfWork.Attendances.UpdateAsync(attendance);
        }
        else
        {
            attendance = new Attendance
            {
                EmployeeId = employeeId,
                CheckIn = date.Date,
                WorkingBranchId = employee.BranchId,
                Shift = ShiftType.Morning,
                AttendanceStatus = AttendanceStatus.Present
            };
            await _unitOfWork.Attendances.AddAsync(attendance);
        }

        // Reversal Logic: subtract (Basic Salary / 30) from total deductions
        decimal penaltyAmount = Math.Round(employee.BasicSalary / 30m, 2);

        var monthlySalary = await _unitOfWork.MonthlySalaries.GetByEmployeeMonthYearAsync(employeeId, date.Month, date.Year);
        if (monthlySalary != null)
        {
            // Reverse deduction
            var absenceItem = monthlySalary.Deductions.FirstOrDefault(d =>
                d.DeductionType == DeductionType.Absence &&
                d.Description.Contains(date.ToString("yyyy-MM-dd")));

            if (wasAbsent || absenceItem != null)
            {
                monthlySalary.TotalDeductions = Math.Max(0, monthlySalary.TotalDeductions - penaltyAmount);
                if (absenceItem != null)
                    absenceItem.IsDeleted = true;
            }

            await _unitOfWork.MonthlySalaries.UpdateAsync(monthlySalary);
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<AttendanceDto>(attendance);
    }

    private static void EnsureAttendanceDateAllowed(Employee employee, DateTime date)
    {
        var day = date.Date;
        if (day < employee.HireDate.Date)
            throw new InvalidOperationException("لا يمكن تسجيل حضور أو غياب قبل تاريخ توظيف الموظف");

        if (employee.TerminationDate.HasValue && day > employee.TerminationDate.Value.Date)
            throw new InvalidOperationException("لا يمكن تسجيل حضور أو غياب بعد تاريخ انتهاء خدمة الموظف");
    }
}
