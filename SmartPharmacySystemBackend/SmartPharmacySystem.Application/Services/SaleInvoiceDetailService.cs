using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class SaleInvoiceDetailService : ISaleInvoiceDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaleInvoiceDetailService> _logger;

        public SaleInvoiceDetailService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleInvoiceDetailService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SaleInvoiceDetailDto> CreateAsync(CreateSaleInvoiceDetailDto dto)
        {

            // 1. Validation & Stock Check
            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(dto.BatchId)
                 ?? throw new KeyNotFoundException($"الدفعة برقم {dto.BatchId} غير موجودة");

            if (batch.RemainingQuantity < dto.Quantity)
                throw new InvalidOperationException($"الكمية المطلوبة ({dto.Quantity}) أكبر من الكمية المتاحة ({batch.RemainingQuantity})");

            var detail = _mapper.Map<SaleInvoiceDetail>(dto);

            // 2. Add Detail
            await _unitOfWork.SaleInvoiceDetails.AddAsync(detail);

            // 3. Update Parent Invoice
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(dto.SaleInvoiceId);
            if (invoice != null)
            {
                // Assuming SaleInvoice has TotalAmount, TotalCost, TotalProfit columns based on User Request.
                // We add to them.
                decimal saleAmount = dto.Quantity * dto.SalePrice;
                decimal costAmount = dto.Quantity * dto.UnitCost;

                invoice.TotalAmount += saleAmount;
                invoice.TotalCost += costAmount;
                invoice.TotalProfit += (saleAmount - costAmount);

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

                // Update Parent
                var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(detail.SaleInvoiceId);
                if (invoice != null)
                {
                    decimal saleAmount = detail.Quantity * detail.SalePrice;
                    decimal costAmount = detail.Quantity * detail.UnitCost;

                    invoice.TotalAmount -= saleAmount;
                    invoice.TotalCost -= costAmount;
                    invoice.TotalProfit -= (saleAmount - costAmount);
                    await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
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
