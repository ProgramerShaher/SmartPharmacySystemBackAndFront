using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Suppliers;
using SmartPharmacySystem.Application.DTOs.SupplierPayments;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;
        private readonly IAccountService _accountService;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SupplierService> logger, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _accountService = accountService;
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplier = _mapper.Map<Supplier>(dto);
                supplier.CreatedAt = DateTime.UtcNow;
                supplier.IsDeleted = false;

                // 1. إنشاء حساب للمورد في شجرة الحسابات
                var parentAccount = await _unitOfWork.Accounts.GetByCodeAsync("2101");
                var lastAccount = (await _unitOfWork.Accounts.GetChildrenAsync(2101)).OrderByDescending(a => a.Code).FirstOrDefault();

                string nextCode = "2101001";
                if (lastAccount != null && long.TryParse(lastAccount.Code, out long lastCode))
                {
                    nextCode = (lastCode + 1).ToString();
                }

                var supplierAccount = new Account
                {
                    Code = nextCode,
                    Name = $"مورد: {supplier.Name}",
                    Type = Core.Enums.AccountType.Liability,
                    ParentId = parentAccount?.Id ?? 2101, // fallback to seeded ID
                    IsMainAccount = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Accounts.AddAsync(supplierAccount);
                await _unitOfWork.SaveChangesAsync();

                // 2. ربط المورد بالحساب
                supplier.AccountId = supplierAccount.Id;
                await _unitOfWork.Suppliers.AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                return _mapper.Map<SupplierDto>(supplier);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating supplier with account");
                throw;
            }
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

        public async Task<SupplierStatementDto> GetStatementAsync(int supplierId)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId)
                ?? throw new KeyNotFoundException("المورد غير موجود");

            if (!supplier.AccountId.HasValue)
                throw new InvalidOperationException("هذا المورد ليس له حساب مرتبط في شجرة الحسابات");

            // جلب دفتر الأستاذ من المحرك المحاسبي
            var ledger = await _accountService.GetGeneralLedgerAsync(
                supplier.AccountId.Value,
                DateTime.Now.AddYears(-1),
                DateTime.Now);

            var result = new SupplierStatementDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                TotalBalance = ledger.ClosingBalance,
                Status = ledger.ClosingBalance == 0 ? "خالص" : "دائن",
                Transactions = ledger.Entries.Select(e => new StatementItemDto
                {
                    Date = e.Date,
                    Type = "قيد محاسبي",
                    Reference = e.VoucherNumber,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    RunningBalance = e.RunningBalance,
                    Notes = e.Description
                }).ToList()
            };

            return result;
        }
    }
}
