using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Suppliers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SupplierService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = _mapper.Map<Supplier>(dto);
            supplier.CreatedAt = DateTime.UtcNow;
            supplier.IsDeleted = false;

            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المورد برقم {id} غير موجود");

            _mapper.Map(dto, supplier);
            supplier.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المورد برقم {id} غير موجود");

            // 1. Balance check
            if (supplier.Balance != 0)
                throw new InvalidOperationException("لا يمكن حذف مورد لديه رصيد مستحق (مديونية أو دائنية).");

            // 2. Transaction check (Alternative: check related collections if loaded, or use specific repo check)
            var hasInvoices = (await _unitOfWork.PurchaseInvoices.GetAllAsync()).Any(i => i.SupplierId == id && !i.IsDeleted);
            if (hasInvoices)
                throw new InvalidOperationException("لا يمكن حذف مورد لديه فواتير شراء مسجلة.");

            await _unitOfWork.Suppliers.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SupplierDto?> GetByIdAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier == null) return null;
            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        public async Task<PagedResult<SupplierDto>> SearchAsync(SupplierQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.Suppliers.GetPagedAsync(
                query.Search,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDirection,
                query.HasBalance);

            var dtos = _mapper.Map<IEnumerable<SupplierDto>>(items);
            return new PagedResult<SupplierDto>(dtos, totalCount, query.Page, query.PageSize);
        }
    }
}
