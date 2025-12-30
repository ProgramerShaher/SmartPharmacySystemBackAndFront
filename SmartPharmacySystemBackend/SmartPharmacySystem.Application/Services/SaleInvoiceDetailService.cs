using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class SaleInvoiceDetailService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleInvoiceDetailService> logger) : ISaleInvoiceDetailService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SaleInvoiceDetailService> _logger = logger;


        public async Task<SaleInvoiceDetailDto> CreateAsync(CreateSaleInvoiceDetailDto dto)
        {

            // 1. Validation & Stock Check
            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(dto.BatchId)
                 ?? throw new KeyNotFoundException($"الدفعة برقم {dto.BatchId} غير موجودة");

            if (batch.RemainingQuantity < dto.Quantity)
                throw new InvalidOperationException($"الكمية المطلوبة ({dto.Quantity}) أكبر من الكمية المتاحة ({batch.RemainingQuantity})");

            var detail = _mapper.Map<SaleInvoiceDetail>(dto);

            // 2. Set Row-Level Totals
            detail.TotalLineAmount = detail.Quantity * detail.SalePrice;
            detail.TotalCost = detail.Quantity * detail.UnitCost;
            detail.Profit = detail.TotalLineAmount - detail.TotalCost;

            // 3. Add Detail
            await _unitOfWork.SaleInvoiceDetails.AddAsync(detail);

            // 4. Update Parent Invoice Totals by re-calculating everything (Master-Detail Sync)
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(dto.SaleInvoiceId)
                 ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {dto.SaleInvoiceId} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن إضافة أصناف لفاتورة معتمدة أو ملغاة. التعديل مسموح فقط لحالة مسودة (Draft).");

            if (invoice != null)
            {
                var allDetails = await _unitOfWork.SaleInvoiceDetails.GetDetailsByInvoiceIdAsync(invoice.Id);
                // The newly added detail is already in the List returned by DB if SaveChanges wasn't called?
                // Actually it's better to calculate after SaveChanges or include the current detail in our manual SUM.
                // But for safety and to follow user formula:
                invoice.TotalAmount = allDetails.Sum(d => d.TotalLineAmount);
                invoice.TotalCost = allDetails.Sum(d => d.TotalCost);
                invoice.TotalProfit = allDetails.Sum(d => d.Profit);

                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
            }

            await _unitOfWork.SaveChangesAsync();

            // 5. Return Full DTO
            var createdDetail = await _unitOfWork.SaleInvoiceDetails.GetByIdAsync(detail.Id);
            return _mapper.Map<SaleInvoiceDetailDto>(createdDetail);
        }

        public async Task UpdateAsync(int id, UpdateSaleInvoiceDetailDto dto)
        {
            var detail = await _unitOfWork.SaleInvoiceDetails.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"تفصيل فاتورة المبيعات برقم {id} غير موجود");

            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(detail.SaleInvoiceId)
                 ?? throw new KeyNotFoundException($"الفاتورة الرئيسية غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن تعديل أصناف فاتورة معتمدة أو ملغاة. التعديل مسموح فقط لحالة مسودة (Draft).");

            _mapper.Map(dto, detail);
            await _unitOfWork.SaleInvoiceDetails.UpdateAsync(detail);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var detail = await _unitOfWork.SaleInvoiceDetails.GetByIdAsync(id);
            if (detail != null)
            {
                // Update Parent

                // Update Parent Invoice Totals by re-calculating (Master-Detail Sync)
                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(detail.SaleInvoiceId)
                     ?? throw new KeyNotFoundException($"الفاتورة الرئيسية غير موجودة");

                if (invoice.Status != DocumentStatus.Draft)
                    throw new InvalidOperationException("لا يمكن حذف أصناف من فاتورة معتمدة أو ملغاة. التعديل مسموح فقط لحالة مسودة (Draft).");

                await _unitOfWork.SaleInvoiceDetails.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                if (invoice != null)
                {
                    var allDetails = await _unitOfWork.SaleInvoiceDetails.GetDetailsByInvoiceIdAsync(invoice.Id);
                    invoice.TotalAmount = allDetails.Sum(d => d.TotalLineAmount);
                    invoice.TotalCost = allDetails.Sum(d => d.TotalCost);
                    invoice.TotalProfit = allDetails.Sum(d => d.Profit);
                    await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.SaleInvoiceDetails.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"تفصيل فاتورة المبيعات برقم {id} غير موجود");
            }
        }

        public async Task<SaleInvoiceDetailDto> GetByIdAsync(int id)
        {
            var detail = await _unitOfWork.SaleInvoiceDetails.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"تفصيل فاتورة المبيعات برقم {id} غير موجود");
            return _mapper.Map<SaleInvoiceDetailDto>(detail);
        }

        public async Task<IEnumerable<SaleInvoiceDetailDto>> GetAllAsync()
        {
            var details = await _unitOfWork.SaleInvoiceDetails.GetAllAsync();
            return _mapper.Map<IEnumerable<SaleInvoiceDetailDto>>(details);
        }
    }
}
