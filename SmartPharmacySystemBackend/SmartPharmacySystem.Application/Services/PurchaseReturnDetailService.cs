using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class PurchaseReturnDetailService : IPurchaseReturnDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseReturnDetailService> _logger;

        public PurchaseReturnDetailService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseReturnDetailService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PurchaseReturnDetailDto> CreateAsync(CreatePurchaseReturnDetailDto dto)
        {
            var detail = _mapper.Map<PurchaseReturnDetail>(dto);
            await _unitOfWork.PurchaseReturnDetails.AddAsync(detail);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PurchaseReturnDetailDto>(detail);
        }

        public async Task UpdateAsync(int id, UpdatePurchaseReturnDetailDto dto)
        {
            var detail = await _unitOfWork.PurchaseReturnDetails.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"تفصيل مرتجع الشراء برقم {id} غير موجود");

            _mapper.Map(dto, detail);
            await _unitOfWork.PurchaseReturnDetails.UpdateAsync(detail);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var detail = await _unitOfWork.PurchaseReturnDetails.GetByIdAsync(id);
            if (detail != null)
            {
                // Update Parent

                // Update Parent
                var parentReturn = await _unitOfWork.PurchaseReturns.GetByIdAsync(detail.PurchaseReturnId);
                if (parentReturn != null)
                {
                    parentReturn.TotalAmount -= (detail.Quantity * detail.PurchasePrice);
                    await _unitOfWork.PurchaseReturns.UpdateAsync(parentReturn);
                }

                await _unitOfWork.PurchaseReturnDetails.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"تفصيل مرتجع الشراء برقم {id} غير موجود");
            }
        }

        public async Task<PurchaseReturnDetailDto> GetByIdAsync(int id)
        {
            var detail = await _unitOfWork.PurchaseReturnDetails.GetByIdAsync(id)
                 ?? throw new KeyNotFoundException($"تفصيل مرتجع الشراء برقم {id} غير موجود");
            return _mapper.Map<PurchaseReturnDetailDto>(detail);
        }

        public async Task<IEnumerable<PurchaseReturnDetailDto>> GetAllAsync()
        {
            var details = await _unitOfWork.PurchaseReturnDetails.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseReturnDetailDto>>(details);
        }
    }
}
