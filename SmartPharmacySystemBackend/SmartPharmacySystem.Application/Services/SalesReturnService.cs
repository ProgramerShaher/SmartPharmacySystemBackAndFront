using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class SalesReturnService : ISalesReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SalesReturnService> _logger;
        private readonly IStockMovementService _stockMovementService;
        private readonly IFinancialService _financialService;

        public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SalesReturnService> logger, IStockMovementService stockMovementService, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _financialService = financialService;
        }

        public async Task<SalesReturnDto> CreateAsync(CreateSalesReturnDto dto)
        {
            var entity = _mapper.Map<SalesReturn>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = DocumentStatus.Draft; // Always start as Draft

            await _unitOfWork.SalesReturns.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.SalesReturns.GetByIdAsync(entity.Id);
            return _mapper.Map<SalesReturnDto>(created);
        }

        public async Task UpdateAsync(int id, UpdateSalesReturnDto dto)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل مرتجع تم اعتماده أو إلغاؤه.");

            _mapper.Map(dto, entity);
            await _unitOfWork.SalesReturns.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("المرتجع بالفعل معتمد أو ملغى.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                ret.Status = DocumentStatus.Approved;
                await _unitOfWork.SalesReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();

                // Trigger Side Effect
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SalesReturn);

                // Financial Integration
                await _financialService.ProcessTransactionAsync(
                    ret.TotalAmount,
                    FinancialTransactionType.Expense,
                    $"Sales Return Approved for Invoice: {ret.SaleInvoiceId}",
                    ret.Id,
                    FinancialInvoiceType.Sale // Categorized under Sales module
                );

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving sales return {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id)
        {
            var ret = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (ret.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("الالمرتجع ملغى بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = ret.Status == DocumentStatus.Approved;
                ret.Status = DocumentStatus.Cancelled;
                await _unitOfWork.SalesReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();

                if (wasApproved)
                {
                    // Trigger Reverse Side Effect
                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.SalesReturn);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sales return {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف مرتجع تم اعتماده. يجب إلغاؤه بدلاً من ذلك.");

            await _unitOfWork.SalesReturns.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SalesReturnDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SalesReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع المبيعات برقم {id} غير موجود");
            return _mapper.Map<SalesReturnDto>(entity);
        }

        public async Task<IEnumerable<SalesReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.SalesReturns.GetAllAsync();
            return _mapper.Map<IEnumerable<SalesReturnDto>>(entities);
        }
    }
}
