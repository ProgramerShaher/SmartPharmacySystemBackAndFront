using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.PurchaseReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseReturnService> _logger;
        private readonly IStockMovementService _stockMovementService;
        private readonly IFinancialService _financialService;

        public PurchaseReturnService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseReturnService> logger, IStockMovementService stockMovementService, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _financialService = financialService;
        }

        public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto)
        {
            var entity = _mapper.Map<PurchaseReturn>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = dto.CreatedBy;
            entity.Status = DocumentStatus.Draft; // Always start as Draft

            await _unitOfWork.PurchaseReturns.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.PurchaseReturns.GetByIdAsync(entity.Id);
            return _mapper.Map<PurchaseReturnDto>(created);
        }

        public async Task UpdateAsync(int id, UpdatePurchaseReturnDto dto)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع الشراء برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل مرتجع تم اعتماده أو إلغاؤه.");

            _mapper.Map(dto, entity);
            await _unitOfWork.PurchaseReturns.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var ret = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع الشراء برقم {id} غير موجود");

            if (ret.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("المرتجع بالفعل معتمد أو ملغى.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                ret.Status = DocumentStatus.Approved;
                await _unitOfWork.PurchaseReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();

                // Trigger Side Effect
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.PurchaseReturn);

                // Financial Integration
                await _financialService.ProcessTransactionAsync(
                    ret.TotalAmount,
                    FinancialTransactionType.Income,
                    $"Purchase Return Approved for Invoice: {ret.PurchaseInvoiceId}",
                    ret.Id,
                    FinancialInvoiceType.Purchase // Categorized under Purchase module
                );

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving purchase return {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id)
        {
            var ret = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع الشراء برقم {id} غير موجود");

            if (ret.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("المرتجع ملغى بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = ret.Status == DocumentStatus.Approved;
                ret.Status = DocumentStatus.Cancelled;
                await _unitOfWork.PurchaseReturns.UpdateAsync(ret);
                await _unitOfWork.SaveChangesAsync();

                if (wasApproved)
                {
                    // Trigger Reverse Side Effect
                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.PurchaseReturn);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling purchase return {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع الشراء برقم {id} غير موجود");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف مرتجع تم اعتماده. يجب إلغاؤه بدلاً من ذلك.");

            await _unitOfWork.PurchaseReturns.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PurchaseReturnDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.PurchaseReturns.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"مرتجع الشراء برقم {id} غير موجود");
            return _mapper.Map<PurchaseReturnDto>(entity);
        }

        public async Task<IEnumerable<PurchaseReturnDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.PurchaseReturns.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseReturnDto>>(entities);
        }
    }
}
