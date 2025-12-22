using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class SaleInvoiceService : ISaleInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaleInvoiceService> _logger;
        private readonly IStockMovementService _stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IFinancialService _financialService;

        public SaleInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleInvoiceService> logger, IStockMovementService stockMovementService, IInvoiceNumberGenerator invoiceNumberGenerator, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _financialService = financialService;
        }

        public async Task<SaleInvoiceDto> CreateAsync(CreateSaleInvoiceDto dto)
        {
            var entity = _mapper.Map<SaleInvoice>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = dto.CreatedBy;
            entity.Status = DocumentStatus.Draft; // Always start as Draft

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                entity.SaleInvoiceNumber = await _invoiceNumberGenerator.GenerateSaleInvoiceNumberAsync();
                await _unitOfWork.SaleInvoices.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var created = await _unitOfWork.SaleInvoices.GetByIdAsync(entity.Id);
            return _mapper.Map<SaleInvoiceDto>(created);
        }

        public async Task UpdateAsync(int id, UpdateSaleInvoiceDto dto)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل فاتورة تم اعتمادها أو إلغاؤها.");

            _mapper.Map(dto, entity);
            await _unitOfWork.SaleInvoices.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("الفاتورة بالفعل معتمدة أو ملغاة.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                invoice.Status = DocumentStatus.Approved;
                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Trigger Side Effect (StockMovements will handle stock/expiry validation internally)
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SaleInvoice);

                // Financial Integration
                await _financialService.ProcessTransactionAsync(
                    invoice.TotalAmount,
                    FinancialTransactionType.Income,
                    $"Sale Invoice Approved: {invoice.SaleInvoiceNumber}",
                    invoice.Id,
                    FinancialInvoiceType.Sale
                );

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving sale invoice {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (invoice.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("الفاتورة ملغاة بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = invoice.Status == DocumentStatus.Approved;
                invoice.Status = DocumentStatus.Cancelled;
                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                if (wasApproved)
                {
                    // Trigger Reverse Side Effect
                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.SaleInvoice);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sale invoice {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف فاتورة تم اعتمادها. يجب إلغاؤها بدلاً من ذلك.");

            await _unitOfWork.SaleInvoices.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SaleInvoiceDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");
            return _mapper.Map<SaleInvoiceDto>(entity);
        }

        public async Task<IEnumerable<SaleInvoiceDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.SaleInvoices.GetAllAsync();
            return _mapper.Map<IEnumerable<SaleInvoiceDto>>(entities);
        }
    }
}
