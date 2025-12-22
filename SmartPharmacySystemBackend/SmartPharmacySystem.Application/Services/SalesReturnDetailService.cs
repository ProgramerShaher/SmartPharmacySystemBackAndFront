using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesReturnDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class SalesReturnDetailService : ISalesReturnDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SalesReturnDetailService> _logger;

        public SalesReturnDetailService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SalesReturnDetailService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SalesReturnDetailDto> CreateAsync(CreateSalesReturnDetailDto dto)
        {
            // 1. Map & Add Detail
            var detail = _mapper.Map<SalesReturnDetail>(dto);
            await _unitOfWork.SalesReturnDetails.AddAsync(detail);

            // 2. Update Parent Return Total
            var parentReturn = await _unitOfWork.SalesReturns.GetByIdAsync(dto.SalesReturnId);
            if (parentReturn != null)
            {
                // Assuming SalePrice used for return calc
                parentReturn.TotalAmount += (dto.Quantity * dto.SalePrice);
                await _unitOfWork.SalesReturns.UpdateAsync(parentReturn);

                // 4. Update Original Sale Invoice
                // Need to find Original Invoice from Parent Return?
                var originalInvoice = await _unitOfWork.SaleInvoices.GetByIdAsync(parentReturn.SaleInvoiceId);
                if (originalInvoice != null)
                {
                    // User requested:
                    // saleInvoice.totalAmount -= totalReturnSale
                    // saleInvoice.totalCost   -= totalReturnCost
                    // saleInvoice.totalProfit -= profitReturn

                    decimal totalReturnSale = dto.Quantity * dto.SalePrice;
                    decimal totalReturnCost = dto.Quantity * dto.UnitCost;
                    decimal profitReturn = totalReturnSale - totalReturnCost;

                    originalInvoice.TotalAmount -= totalReturnSale;
                    originalInvoice.TotalCost -= totalReturnCost;
                    originalInvoice.TotalProfit -= profitReturn;
                    await _unitOfWork.SaleInvoices.UpdateAsync(originalInvoice);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // 5. Return Full DTO
            var createdDetail = await _unitOfWork.SalesReturnDetails.GetByIdAsync(detail.Id);
            return _mapper.Map<SalesReturnDetailDto>(createdDetail);
        }

        public async Task UpdateAsync(int id, UpdateSalesReturnDetailDto dto)
        {
            var detail = await _unitOfWork.SalesReturnDetails.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"تفصيل مرتجع المبيعات برقم {id} غير موجود");

            _mapper.Map(dto, detail);
            await _unitOfWork.SalesReturnDetails.UpdateAsync(detail);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var detail = await _unitOfWork.SalesReturnDetails.GetByIdAsync(id);
            if (detail != null)
            {
                // Update Parent

                // Update Parent
                var parentReturn = await _unitOfWork.SalesReturns.GetByIdAsync(detail.SalesReturnId);
                if (parentReturn != null)
                {
                    decimal totalReturnSale = detail.Quantity * detail.SalePrice;
                    parentReturn.TotalAmount -= totalReturnSale;
                    await _unitOfWork.SalesReturns.UpdateAsync(parentReturn);

                    // Revert Original Invoice Update
                    var originalInvoice = await _unitOfWork.SaleInvoices.GetByIdAsync(parentReturn.SaleInvoiceId);
                    if (originalInvoice != null)
                    {
                        decimal totalReturnCost = detail.Quantity * detail.UnitCost;
                        decimal profitReturn = totalReturnSale - totalReturnCost;

                        originalInvoice.TotalAmount += totalReturnSale;
                        originalInvoice.TotalCost += totalReturnCost;
                        originalInvoice.TotalProfit += profitReturn;
                        await _unitOfWork.SaleInvoices.UpdateAsync(originalInvoice);
                    }
                }

                await _unitOfWork.SalesReturnDetails.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"تفصيل مرتجع المبيعات برقم {id} غير موجود");
            }
        }

        public async Task<SalesReturnDetailDto> GetByIdAsync(int id)
        {
            var detail = await _unitOfWork.SalesReturnDetails.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"تفصيل مرتجع المبيعات برقم {id} غير موجود");
            return _mapper.Map<SalesReturnDetailDto>(detail);
        }

        public async Task<IEnumerable<SalesReturnDetailDto>> GetAllAsync()
        {
            var details = await _unitOfWork.SalesReturnDetails.GetAllAsync();
            return _mapper.Map<IEnumerable<SalesReturnDetailDto>>(details);
        }
    }
}
