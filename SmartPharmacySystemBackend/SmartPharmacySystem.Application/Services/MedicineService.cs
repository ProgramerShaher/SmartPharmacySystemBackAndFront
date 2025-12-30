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
            var exists = await _unitOfWork.Medicines.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            await _unitOfWork.Medicines.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<MedicineDto> GetMedicineByIdAsync(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            return _mapper.Map<MedicineDto>(medicine);
        }

        public async Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            return _mapper.Map<IEnumerable<MedicineDto>>(medicines);
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
