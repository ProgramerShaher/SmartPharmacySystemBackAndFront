using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineService> _logger;

        public MedicineService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicineService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto)
        {
            var medicine = _mapper.Map<Medicine>(dto);
            medicine.CreatedAt = DateTime.UtcNow;
            medicine.IsDeleted = false;

            await _unitOfWork.Medicines.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MedicineDto>(medicine);
        }

        public async Task UpdateMedicineAsync(int id, UpdateMedicineDto dto)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            _mapper.Map(dto, medicine);
            medicine.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Medicines.UpdateAsync(medicine);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteMedicineAsync(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (medicine == null)
                throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            // 1. التحقق من وجود مخزون
            var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(id);
            var totalStock = batches.Sum(b => b.RemainingQuantity);
            if (totalStock > 0)
                throw new InvalidOperationException($"لا يمكن حذف الدواء '{medicine.Name}' لوجود مخزون حالي ({totalStock})");

            // 2. التحقق من وجود عمليات سابقة (مبيعات أو مشتريات)
            // يمكن التحقق من حركات المخزون كمرجع شامل
            var movements = await _unitOfWork.InventoryMovements.GetStockCardMovementsAsync(id);
            if (movements.Any())
            {
                // بدلاً من الحذف، نقوم بتغيير الحالة إلى غير نشط
                medicine.Status = "Inactive";
                medicine.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Medicines.UpdateAsync(medicine);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            // 3. الحذف المنطقي النهائي إذا لم تكن هناك قيود
            await _unitOfWork.Medicines.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<MedicineDto> GetMedicineByIdAsync(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            var dto = _mapper.Map<MedicineDto>(medicine);
            
            // حساب المخزون الكلي
            var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(id);
            dto.TotalStock = batches.Sum(b => b.RemainingQuantity);
            dto.PurchasePrice = medicine.DefaultPurchasePrice;
            dto.SalePrice = medicine.DefaultSalePrice;

            return dto;
        }

        public async Task<MedicineDetailsDto> GetMedicineDetailsAsync(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            var dto = _mapper.Map<MedicineDetailsDto>(medicine);
            
            var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(id);
            dto.TotalStock = batches.Sum(b => b.RemainingQuantity);
            dto.PurchasePrice = medicine.DefaultPurchasePrice;
            dto.SalePrice = medicine.DefaultSalePrice;

            dto.Batches = batches.Select(b => new MedicineBatchDetailDto
            {
                BatchNumber = b.CompanyBatchNumber,
                ExpiryDate = b.ExpiryDate,
                RemainingQuantity = b.RemainingQuantity,
                AlertStatus = (b.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "قريب الانتهاء" : "صالح",
                StatusColor = (b.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "danger" : "success"
            }).ToList();

            return dto;
        }

        public async Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MedicineDto>>(medicines);
            
            foreach (var dto in dtos)
            {
                var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(dto.Id);
                dto.TotalStock = batches.Sum(b => b.RemainingQuantity);
                
                var med = medicines.First(m => m.Id == dto.Id);
                dto.PurchasePrice = med.DefaultPurchasePrice;
                dto.SalePrice = med.DefaultSalePrice;
            }

            return dtos;
        }

        public async Task<PagedResult<MedicineDto>> SearchAsync(MedicineQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.Medicines.GetPagedAsync(
                query.Search,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDirection,
                query.CategoryId,
                query.Manufacturer,
                query.Status
            );

            var mappedItems = _mapper.Map<IEnumerable<MedicineDto>>(items);
            
            foreach (var dto in mappedItems)
            {
                var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(dto.Id);
                dto.TotalStock = batches.Sum(b => b.RemainingQuantity);
                
                var med = items.First(m => m.Id == dto.Id);
                dto.PurchasePrice = med.DefaultPurchasePrice;
                dto.SalePrice = med.DefaultSalePrice;
            }

            return new PagedResult<MedicineDto>(mappedItems, totalCount, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<SmartPharmacySystem.Application.DTOs.MedicineBatch.MedicineBatchResponseDto>> GetBatchesByFEFOAsync(int medicineId)
        {
            var batches = await _unitOfWork.Medicines.GetBatchesByFEFOAsync(medicineId);
            return _mapper.Map<IEnumerable<SmartPharmacySystem.Application.DTOs.MedicineBatch.MedicineBatchResponseDto>>(batches);
        }

        public async Task<IEnumerable<MedicineDto>> GetReorderReportAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetReorderReadyMedicinesAsync();
            return _mapper.Map<IEnumerable<MedicineDto>>(medicines);
        }
    }
}
