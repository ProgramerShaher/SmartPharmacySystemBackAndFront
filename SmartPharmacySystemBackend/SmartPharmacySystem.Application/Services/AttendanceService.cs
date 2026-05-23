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
}
