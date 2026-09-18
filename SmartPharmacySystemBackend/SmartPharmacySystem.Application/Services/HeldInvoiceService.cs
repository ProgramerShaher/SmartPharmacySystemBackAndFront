using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.HeldInvoices;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Services
{
    public class HeldInvoiceService : IHeldInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<HeldInvoiceService> _logger;

        public HeldInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<HeldInvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<HeldInvoiceDto> HoldAsync(CreateHeldInvoiceDto dto, int userId, int? branchId)
        {
            if (string.IsNullOrWhiteSpace(dto.CartJson))
            {
                throw new InvalidOperationException("لا يمكن تعليق فاتورة فارغة لا تحتوي على أصناف.");
            }

            var refName = string.IsNullOrWhiteSpace(dto.HoldReference)
                ? $"معلقة #{DateTime.Now:HH:mm:ss} - {(string.IsNullOrWhiteSpace(dto.CustomerName) ? "زبون نقدي" : dto.CustomerName)}"
                : dto.HoldReference;

            var entity = new HeldInvoice
            {
                HoldReference = refName,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                UserShiftId = dto.UserShiftId,
                TotalAmount = dto.TotalAmount,
                TotalDiscount = dto.TotalDiscount,
                ItemsCount = dto.ItemsCount,
                CartJson = dto.CartJson,
                Notes = dto.Notes,
                BranchId = branchId ?? 1,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HeldInvoices.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Held invoice created successfully: Id={Id}, Ref={Ref}, BranchId={BranchId}", entity.Id, entity.HoldReference, entity.BranchId);

            return _mapper.Map<HeldInvoiceDto>(entity);
        }

        public async Task<IEnumerable<HeldInvoiceDto>> GetHeldInvoicesAsync(int? branchId)
        {
            var entities = await _unitOfWork.HeldInvoices.GetAllActiveAsync(branchId);
            return _mapper.Map<IEnumerable<HeldInvoiceDto>>(entities);
        }

        public async Task<HeldInvoiceDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.HeldInvoices.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<HeldInvoiceDto>(entity);
        }

        public async Task<bool> ResumeAndDeleteAsync(int id)
        {
            var entity = await _unitOfWork.HeldInvoices.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.HeldInvoices.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
