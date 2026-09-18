using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.ProductVariants;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductVariantService> _logger;

        public ProductVariantService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductVariantService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductVariantDto>> GetByMedicineIdAsync(int medicineId)
        {
            var variants = await _unitOfWork.ProductVariants.GetByMedicineIdAsync(medicineId);
            return _mapper.Map<IEnumerable<ProductVariantDto>>(variants);
        }

        public async Task<ProductVariantDto?> GetByIdAsync(int id)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(id);
            return variant == null ? null : _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task<ProductVariantDto?> GetByBarcodeOrSkuAsync(string code)
        {
            var variant = await _unitOfWork.ProductVariants.GetByBarcodeOrSkuAsync(code);
            return variant == null ? null : _mapper.Map<ProductVariantDto>(variant);
        }

        public async Task<ProductVariantDto> CreateAsync(CreateProductVariantDto dto)
        {
            var entity = _mapper.Map<ProductVariant>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ProductVariants.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("ProductVariant created: Id={Id}, MedicineId={MedicineId}, Size={Size}, Color={Color}",
                entity.Id, entity.MedicineId, entity.Size, entity.Color);

            return _mapper.Map<ProductVariantDto>(entity);
        }

        public async Task<ProductVariantDto> UpdateAsync(int id, UpdateProductVariantDto dto)
        {
            var entity = await _unitOfWork.ProductVariants.GetByIdAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"تنويعة الصنف برقم {id} غير موجودة");
            }

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ProductVariants.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("ProductVariant updated: Id={Id}", id);

            return _mapper.Map<ProductVariantDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.ProductVariants.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.ProductVariants.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("ProductVariant soft-deleted: Id={Id}", id);
            return true;
        }
    }
}
