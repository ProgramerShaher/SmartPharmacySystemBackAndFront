using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using ExcelDataReader;
using System.Data;

using Microsoft.Extensions.Caching.Memory;

namespace SmartPharmacySystem.Application.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineService> _logger;
        private readonly IMemoryCache _cache;

        private const string MedicineLookupCacheKey = "lookup_medicines_cache";

        public MedicineService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicineService> logger, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto dto)
        {
            var medicine = _mapper.Map<Medicine>(dto);
            medicine.CreatedAt = DateTime.UtcNow;
            medicine.IsDeleted = false;

            if (medicine.MedicineUnits != null)
            {
                foreach (var unit in medicine.MedicineUnits)
                {
                    unit.CreatedAt = DateTime.UtcNow;
                    unit.IsDeleted = false;
                }
            }

            await _unitOfWork.Medicines.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(MedicineLookupCacheKey);

            return _mapper.Map<MedicineDto>(medicine);
        }


        public async Task UpdateMedicineAsync(int id, UpdateMedicineDto dto)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"الدواء برقم {id} غير موجود");

            _mapper.Map(dto, medicine);
            medicine.UpdatedAt = DateTime.UtcNow;

            // Sync units
            if (dto.MedicineUnits != null)
            {
                // Remove units that are not in the DTO anymore
                var dtoUnitIds = dto.MedicineUnits.Where(u => u.Id > 0).Select(u => u.Id).ToList();
                var unitsToRemove = medicine.MedicineUnits.Where(u => !dtoUnitIds.Contains(u.Id)).ToList();
                foreach (var unit in unitsToRemove)
                {
                    medicine.MedicineUnits.Remove(unit);
                }

                // Add or update units
                foreach (var unitDto in dto.MedicineUnits)
                {
                    if (unitDto.Id > 0)
                    {
                        var existingUnit = medicine.MedicineUnits.FirstOrDefault(u => u.Id == unitDto.Id);
                        if (existingUnit != null)
                        {
                            _mapper.Map(unitDto, existingUnit);
                            existingUnit.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        var newUnit = _mapper.Map<MedicineUnit>(unitDto);
                        newUnit.CreatedAt = DateTime.UtcNow;
                        newUnit.IsDeleted = false;
                        medicine.MedicineUnits.Add(newUnit);
                    }
                }
            }

            await _unitOfWork.Medicines.UpdateAsync(medicine);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(MedicineLookupCacheKey);
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
                _cache.Remove(MedicineLookupCacheKey);
                return;
            }

            // 3. الحذف المنطقي النهائي إذا لم تكن هناك قيود
            await _unitOfWork.Medicines.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(MedicineLookupCacheKey);
        }


        public async Task DeleteBulkMedicinesAsync(IEnumerable<int> ids)
        {
            var errors = new List<string>();
            foreach (var id in ids)
            {
                try
                {
                    await DeleteMedicineAsync(id);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            if (errors.Any())
            {
                throw new InvalidOperationException("لم يتم حذف بعض الأدوية:\n" + string.Join("\n", errors));
            }
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
                Id = b.Id,
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

        public async Task<byte[]> GenerateExcelTemplateAsync()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("الاسم التجاري (مطلوب),الاسم العلمي,الصنف,الكود,الباركود,سعر الشراء,سعر البيع,الملاحظات,الوحدة الأساسية");
            sb.AppendLine("بنادول ادفانس,باراسيتامول,مسكنات ألم,MED001,123456789,10,15,مسكن عام,حبة");
            
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return preamble.Concat(data).ToArray();
        }

        public async Task<ImportResultDto> ImportFromExcelAsync(IFormFile file)
        {
            var result = new ImportResultDto();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };
            var dataSet = reader.AsDataSet(conf);
            var dataTable = dataSet.Tables[0];

            var allCategories = (await _unitOfWork.Categories.GetAllAsync()).ToList();
            var allMedicines = (await _unitOfWork.Medicines.GetAllAsync()).ToList();

            var usedBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in allMedicines)
            {
                if (!string.IsNullOrWhiteSpace(m.DefaultBarcode)) usedBarcodes.Add(m.DefaultBarcode);
            }

            int rowNumber = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                rowNumber++;
                result.TotalProcessed++;
                try
                {
                    var name = row[0]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        result.FailedCount++;
                        result.Errors.Add($"الصف {rowNumber}: الاسم التجاري مطلوب.");
                        continue;
                    }

                    var categoryName = row.ItemArray.Length > 2 ? row[2]?.ToString()?.Trim() : null;
                    int? categoryId = null;
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        var category = allCategories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                        if (category == null)
                        {
                            category = new Category { Name = categoryName, Description = "مضاف تلقائياً", CreatedAt = DateTime.UtcNow };
                            await _unitOfWork.Categories.AddAsync(category);
                            await _unitOfWork.SaveChangesAsync();
                            allCategories.Add(category);
                        }
                        categoryId = category.Id;
                    }

                    var internalCode = row.ItemArray.Length > 3 ? row[3]?.ToString()?.Trim() : null;
                    var barcode = row.ItemArray.Length > 4 ? row[4]?.ToString()?.Trim() : null;
                    if (string.IsNullOrWhiteSpace(barcode)) barcode = null;
                    
                    var existingMedicine = allMedicines.FirstOrDefault(m => 
                        m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || 
                        (barcode != null && m.DefaultBarcode == barcode));

                    bool isNew = existingMedicine == null;

                    if (isNew && barcode != null && usedBarcodes.Contains(barcode))
                    {
                        result.FailedCount++;
                        result.Errors.Add($"الصف {rowNumber}: الباركود ({barcode}) مستخدم مسبقاً.");
                        continue;
                    }
                    if (barcode != null) usedBarcodes.Add(barcode);

                    var medicine = isNew ? new Medicine() : existingMedicine;

                    medicine.Name = name;
                    medicine.ScientificName = row.ItemArray.Length > 1 ? row[1]?.ToString()?.Trim() : null;
                    medicine.CategoryId = categoryId;
                    medicine.InternalCode = internalCode;
                    medicine.DefaultBarcode = barcode;
                    medicine.BaseUnitName = row.ItemArray.Length > 8 && !string.IsNullOrWhiteSpace(row[8]?.ToString()) ? row[8].ToString().Trim() : "حبة";
                    
                    if (row.ItemArray.Length > 5 && decimal.TryParse(row[5]?.ToString(), out var pPrice)) medicine.DefaultPurchasePrice = pPrice;
                    if (row.ItemArray.Length > 6 && decimal.TryParse(row[6]?.ToString(), out var sPrice)) medicine.DefaultSalePrice = sPrice;
                    medicine.Notes = row.ItemArray.Length > 7 ? row[7]?.ToString()?.Trim() : null;

                    medicine.UpdatedAt = DateTime.UtcNow;
                    medicine.Status = "Active";

                    if (isNew)
                    {
                        medicine.CreatedAt = DateTime.UtcNow;
                        medicine.IsDeleted = false;
                        await _unitOfWork.Medicines.AddAsync(medicine);
                        result.SuccessCount++;
                    }
                    else
                    {
                        await _unitOfWork.Medicines.UpdateAsync(medicine);
                        result.UpdatedCount++;
                    }
                    
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"الصف {rowNumber}: {ex.Message}");
                    
                    // Break out of the loop completely if EF Core tracker is corrupted, 
                    // but since we pre-validate barcodes, DB errors should be minimal.
                }
            }

            _cache.Remove(MedicineLookupCacheKey);
            return result;
        }

        public async Task<IEnumerable<MedicineDto>> GetLookupMedicinesAsync()
        {
            if (_cache.TryGetValue(MedicineLookupCacheKey, out IEnumerable<MedicineDto>? cached) && cached != null)
            {
                return cached;
            }

            var medicines = await _unitOfWork.Medicines.GetLookupProjectionsAsync();
            var dtos = _mapper.Map<IEnumerable<MedicineDto>>(medicines).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(2))
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(MedicineLookupCacheKey, dtos, cacheOptions);
            return dtos;
        }
    }
}

