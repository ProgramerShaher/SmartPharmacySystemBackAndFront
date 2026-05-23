using AutoMapper;
using SmartPharmacySystem.Application.DTOs.EmployeeLoans;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class EmployeeLoanService : IEmployeeLoanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeLoanService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmployeeLoanDto> GetByIdAsync(int id)
    {
        var loan = await _unitOfWork.EmployeeLoans.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("القرض غير موجود");
        return _mapper.Map<EmployeeLoanDto>(loan);
    }

    public async Task<IEnumerable<EmployeeLoanDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var loans = await _unitOfWork.EmployeeLoans.GetByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<EmployeeLoanDto>>(loans);
    }

    public async Task<IEnumerable<EmployeeLoanDto>> GetActiveLoansAsync(int? branchId = null)
    {
        var loans = await _unitOfWork.EmployeeLoans.GetActiveLoansAsync(branchId);
        return _mapper.Map<IEnumerable<EmployeeLoanDto>>(loans);
    }

    public async Task<IEnumerable<EmployeeLoanDto>> GetFullyPaidLoansAsync()
    {
        var loans = await _unitOfWork.EmployeeLoans.GetFullyPaidLoansAsync();
        return _mapper.Map<IEnumerable<EmployeeLoanDto>>(loans);
    }

    public async Task<EmployeeLoanDto> CreateAsync(CreateEmployeeLoanDto dto)
    {
        var loan = _mapper.Map<EmployeeLoan>(dto);
        loan.IsFullyPaid = false;
        await _unitOfWork.EmployeeLoans.AddAsync(loan);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<EmployeeLoanDto>(loan);
    }

    public async Task UpdateAsync(UpdateEmployeeLoanDto dto)
    {
        var loan = await _unitOfWork.EmployeeLoans.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("القرض غير موجود");

        _mapper.Map(dto, loan);
        await _unitOfWork.EmployeeLoans.UpdateAsync(loan);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await _unitOfWork.EmployeeLoans.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("القرض غير موجود");
        await _unitOfWork.EmployeeLoans.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<EmployeeLoanDto> RecordPaymentAsync(int loanId, decimal amount)
    {
        var loan = await _unitOfWork.EmployeeLoans.GetByIdAsync(loanId)
            ?? throw new KeyNotFoundException("القرض غير موجود");

        if (amount <= 0)
            throw new InvalidOperationException("المبلغ يجب أن يكون أكبر من صفر");

        loan.RemainingAmount -= amount;

        if (loan.RemainingAmount <= 0)
        {
            loan.IsFullyPaid = true;
        }

        await _unitOfWork.EmployeeLoans.UpdateAsync(loan);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<EmployeeLoanDto>(loan);
    }

    public async Task<decimal> GetTotalRemainingAsync(int employeeId)
    {
        return await _unitOfWork.EmployeeLoans.GetTotalRemainingAsync(employeeId);
    }
}
