using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Models;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountService = accountService;
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<PagedResponse<CustomerDto>> GetAllPagedAsync(string? search, int page, int pageSize)
        {
            var (items, total) = await _unitOfWork.Customers.GetPagedAsync(search, page, pageSize);
            var dtos = _mapper.Map<IEnumerable<CustomerDto>>(items);
            return new PagedResponse<CustomerDto>(dtos, total, page, pageSize);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var customer = _mapper.Map<Customer>(dto);
                
                // 1. إنشاء حساب للعميل في شجرة الحسابات
                var parentAccount = await _unitOfWork.Accounts.GetByCodeAsync("1201");
                var lastAccount = (await _unitOfWork.Accounts.GetChildrenAsync(1201)).OrderByDescending(a => a.Code).FirstOrDefault();
                
                string nextCode = "1201001";
                if (lastAccount != null && long.TryParse(lastAccount.Code, out long lastCode))
                {
                    nextCode = (lastCode + 1).ToString();
                }

                var customerAccount = new Account
                {
                    Code = nextCode,
                    Name = $"عميل: {customer.Name}",
                    Type = Core.Enums.AccountType.Asset,
                    ParentId = parentAccount?.Id ?? 1201, // fallback to seeded ID
                    IsMainAccount = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Accounts.AddAsync(customerAccount);
                await _unitOfWork.SaveChangesAsync(); // جلب المعرف للحساب الجديد

                // 2. ربط العميل بالحساب
                customer.AccountId = customerAccount.Id;
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(UpdateCustomerDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException("العميل غير موجود");
            
            _mapper.Map(dto, customer);
            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerDto>> GetTopDebtorsAsync(int count)
        {
            var items = await _unitOfWork.Customers.GetTopDebtorsAsync(count);
            return _mapper.Map<IEnumerable<CustomerDto>>(items);
        }

        public async Task<CustomerStatementDto> GetStatementAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new KeyNotFoundException("العميل غير موجود");

            if (!customer.AccountId.HasValue)
                throw new InvalidOperationException("هذا العميل ليس له حساب مرتبط في شجرة الحسابات");

            // جلب دفتر الأستاذ من المحرك المحاسبي (مثلاً لآخر سنة)
            var ledger = await _accountService.GetGeneralLedgerAsync(
                customer.AccountId.Value, 
                DateTime.Now.AddYears(-1), 
                DateTime.Now);

            var result = new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CurrentBalance = ledger.ClosingBalance,
                Items = ledger.Entries.Select(e => new CustomerStatementItemDto
                {
                    Date = e.Date,
                    Type = "قيد محاسبي", // يمكن تحسينها لاحقاً بناءً على نوع السند
                    Reference = e.VoucherNumber,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    RunningBalance = e.RunningBalance,
                    Notes = e.Description
                }).ToList()
            };

            return result;
        }
        public async Task<CustomerStatistics> GetStatisticsAsync()
        {
            return await _unitOfWork.Customers.GetStatisticsAsync();
        }
    }
}
