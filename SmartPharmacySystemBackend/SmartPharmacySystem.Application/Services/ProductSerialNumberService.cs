using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.ProductSerialNumbers;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Services
{
    public class ProductSerialNumberService : IProductSerialNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductSerialNumberService> _logger;

        public ProductSerialNumberService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductSerialNumberService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductSerialNumberDto>> GetInStockByMedicineIdAsync(int medicineId)
        {
            var list = await _unitOfWork.ProductSerialNumbers.GetInStockByMedicineIdAsync(medicineId);
            return _mapper.Map<IEnumerable<ProductSerialNumberDto>>(list);
        }

        public async Task<WarrantyCheckResultDto?> CheckWarrantyAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return null;

            var entity = await _unitOfWork.ProductSerialNumbers.GetBySerialNumberAsync(serialNumber);
            if (entity == null) return null;

            var now = DateTime.UtcNow;
            var isUnderWarranty = entity.Status == SerialNumberStatus.Sold &&
                                  entity.WarrantyExpiryDate.HasValue &&
                                  entity.WarrantyExpiryDate.Value >= now;

            var remainingDays = 0;
            if (entity.WarrantyExpiryDate.HasValue && entity.WarrantyExpiryDate.Value > now)
            {
                remainingDays = (int)(entity.WarrantyExpiryDate.Value - now).TotalDays;
            }

            return new WarrantyCheckResultDto
            {
                SerialNumber = entity.SerialNumber,
                MedicineId = entity.MedicineId,
                MedicineName = entity.Medicine?.Name ?? "جهاز غير محدد",
                Status = entity.Status.ToString(),
                SaleDate = entity.SaleDate,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer?.Name,
                WarrantyMonths = entity.WarrantyMonths,
                WarrantyExpiryDate = entity.WarrantyExpiryDate,
                IsUnderWarranty = isUnderWarranty,
                RemainingDays = remainingDays,
                Notes = entity.Notes
            };
        }

        public async Task<IEnumerable<ProductSerialNumberDto>> RegisterSerialNumbersAsync(RegisterSerialNumbersDto dto, int userId)
        {
            if (dto.SerialNumbers == null || !dto.SerialNumbers.Any())
            {
                throw new ArgumentException("قائمة الأرقام التسلسلية فارغة.");
            }

            var entitiesToAdd = new List<ProductSerialNumber>();
            var duplicates = new List<string>();

            foreach (var rawSn in dto.SerialNumbers)
            {
                var cleanSn = rawSn?.Trim();
                if (string.IsNullOrWhiteSpace(cleanSn)) continue;

                var exists = await _unitOfWork.ProductSerialNumbers.ExistsAsync(cleanSn);
                if (exists)
                {
                    duplicates.Add(cleanSn);
                    continue;
                }

                var snEntity = new ProductSerialNumber
                {
                    MedicineId = dto.MedicineId,
                    BatchId = dto.BatchId,
                    PurchaseInvoiceDetailId = dto.PurchaseInvoiceDetailId,
                    SerialNumber = cleanSn,
                    Status = SerialNumberStatus.InStock,
                    WarrantyMonths = dto.WarrantyMonths > 0 ? dto.WarrantyMonths : 12,
                    Notes = dto.Notes,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                entitiesToAdd.Add(snEntity);
            }

            if (duplicates.Any())
            {
                _logger.LogWarning("Some serial numbers were skipped because they already exist: {Duplicates}", string.Join(", ", duplicates));
            }

            if (entitiesToAdd.Any())
            {
                await _unitOfWork.ProductSerialNumbers.AddRangeAsync(entitiesToAdd);
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<ProductSerialNumberDto>>(entitiesToAdd);
        }

        public async Task<bool> AssignToSaleAsync(int saleDetailId, IEnumerable<string> serialNumbers, int? customerId, int warrantyMonths)
        {
            if (serialNumbers == null || !serialNumbers.Any()) return true;

            foreach (var rawSn in serialNumbers)
            {
                var cleanSn = rawSn?.Trim();
                if (string.IsNullOrWhiteSpace(cleanSn)) continue;

                var snEntity = await _unitOfWork.ProductSerialNumbers.GetBySerialNumberAsync(cleanSn);
                if (snEntity != null)
                {
                    snEntity.MarkAsSold(saleDetailId, customerId, warrantyMonths);
                    await _unitOfWork.ProductSerialNumbers.UpdateAsync(snEntity);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
