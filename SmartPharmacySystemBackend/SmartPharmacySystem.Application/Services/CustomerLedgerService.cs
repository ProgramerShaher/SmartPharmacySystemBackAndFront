using AutoMapper;
using SmartPharmacySystem.Application.DTOs.CustomerLedgers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class CustomerLedgerService : ICustomerLedgerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerLedgerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerLedgerDto> GetByIdAsync(int id)
    {
        var entry = await _unitOfWork.CustomerLedgers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("القيد غير موجود");
        return _mapper.Map<CustomerLedgerDto>(entry);
    }

    public async Task<IEnumerable<CustomerLedgerDto>> GetByCustomerIdAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var entries = await _unitOfWork.CustomerLedgers.GetByCustomerIdAsync(customerId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<CustomerLedgerDto>>(entries);
    }

    public async Task<IEnumerable<CustomerLedgerDto>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var entries = await _unitOfWork.CustomerLedgers.GetByBranchIdAsync(branchId, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<CustomerLedgerDto>>(entries);
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        return await _unitOfWork.CustomerLedgers.GetCustomerBalanceAsync(customerId);
    }

    public async Task<decimal> GetCustomerBalanceAtBranchAsync(int customerId, int branchId)
    {
        return await _unitOfWork.CustomerLedgers.GetCustomerBalanceAtBranchAsync(customerId, branchId);
    }

    public async Task<CustomerLedgerDto> AddEntryAsync(CreateCustomerLedgerDto dto)
    {
        var entry = _mapper.Map<CustomerLedger>(dto);
        await _unitOfWork.CustomerLedgers.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<CustomerLedgerDto>(entry);
    }

    public async Task<IEnumerable<CustomerLedgerDto>> SearchAsync(int? customerId = null, int? branchId = null, CustomerTransactionType? type = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var entries = await _unitOfWork.CustomerLedgers.SearchAsync(customerId, branchId, type, dateFrom, dateTo);
        return _mapper.Map<IEnumerable<CustomerLedgerDto>>(entries);
    }
}
