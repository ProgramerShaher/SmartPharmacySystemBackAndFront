using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseInvoiceService> _logger;
        private readonly IStockMovementService _stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IFinancialService _financialService;

        public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseInvoiceService> logger, IStockMovementService stockMovementService, IInvoiceNumberGenerator invoiceNumberGenerator, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _stockMovementService = stockMovementService;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _financialService = financialService;
        }

        public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            var invoice = _mapper.Map<PurchaseInvoice>(dto);
            invoice.CreatedAt = DateTime.UtcNow;
            invoice.CreatedBy = dto.CreatedBy;
            invoice.Status = DocumentStatus.Draft; // Always start as Draft

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                invoice.PurchaseInvoiceNumber = await _invoiceNumberGenerator.GeneratePurchaseInvoiceNumberAsync();
                await _unitOfWork.PurchaseInvoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var createdInvoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(invoice.Id);
            return _mapper.Map<PurchaseInvoiceDto>(createdInvoice);
        }

        public async Task UpdateAsync(int id, UpdatePurchaseInvoiceDto dto)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل فاتورة تم اعتمادها أو إلغاؤها.");

            _mapper.Map(dto, invoice);
            await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("الفاتورة بالفعل معتمدة أو ملغاة.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                invoice.Status = DocumentStatus.Approved;
                await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // Trigger Side Effect
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.PurchaseInvoice);

                // Financial Integration
                await _financialService.ProcessTransactionAsync(
                    invoice.TotalAmount,
                    FinancialTransactionType.Expense,
                    $"Purchase Invoice Approved: {invoice.PurchaseInvoiceNumber}",
                    invoice.Id,
                    FinancialInvoiceType.Purchase
                );

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving purchase invoice {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("الفاتورة ملغاة بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = invoice.Status == DocumentStatus.Approved;
                invoice.Status = DocumentStatus.Cancelled;
                await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                if (wasApproved)
                {
                    // Trigger Reverse Side Effect
                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.PurchaseInvoice);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling purchase invoice {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف فاتورة تم اعتمادها. يجب إلغاؤها بدلاً من ذلك.");

            await _unitOfWork.PurchaseInvoices.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PurchaseInvoiceDto> GetByIdAsync(int id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة الشراء برقم {id} غير موجودة");

            return _mapper.Map<PurchaseInvoiceDto>(invoice);
        }

        public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
        {
            var invoices = await _unitOfWork.PurchaseInvoices.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseInvoiceDto>>(invoices);
        }
    }
}
