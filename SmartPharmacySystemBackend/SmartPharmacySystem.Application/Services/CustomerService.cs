using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var customer = _mapper.Map<Customer>(dto);
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CustomerDto>(customer);
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

            var result = new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CurrentBalance = customer.Balance
            };

            var items = new List<CustomerStatementItemDto>();

            // 1. Sale Invoices (Debit)
            var invoices = customer.SaleInvoices
                .Where(i => i.PaymentMethod == Core.Enums.PaymentType.Credit && i.Status == Core.Enums.DocumentStatus.Approved)
                .Select(i => new CustomerStatementItemDto
                {
                    Date = i.InvoiceDate,
                    Type = "فاتورة مبيعات",
                    Reference = i.SaleInvoiceNumber,
                    Debit = i.TotalAmount,
                    Credit = 0,
                    Notes = $"فاتورة رقم {i.SaleInvoiceNumber}"
                });
            items.AddRange(invoices);

            // 2. Receipts (Credit)
            var receipts = customer.Receipts
                .Where(r => !r.IsCancelled)
                .Select(r => new CustomerStatementItemDto
                {
                    Date = r.ReceiptDate,
                    Type = "سند قبض",
                    Reference = r.ReferenceNo ?? r.Id.ToString(),
                    Debit = 0,
                    Credit = r.Amount,
                    Notes = r.Notes
                });
            items.AddRange(receipts);

            // 3. Sales Returns (Credit)
            // Fetch returns linked to this customer's invoices
            var returns = await _unitOfWork.SalesReturns.GetAllAsync(); // This is expensive, better to have a repo method
            var customerReturns = returns
                .Where(r => r.CustomerId == customerId && r.Status == Core.Enums.DocumentStatus.Approved)
                .Select(r => new CustomerStatementItemDto
                {
                    Date = r.ReturnDate,
                    Type = "مرتجع مبيعات",
                    Reference = r.Id.ToString(),
                    Debit = 0,
                    Credit = r.TotalAmount,
                    Notes = "مرتجع مبيعات"
                });
            items.AddRange(customerReturns);

            // Sort by Date
            result.Items = items.OrderBy(i => i.Date).ToList();

            // Calculate Running Balance
            decimal runningBalance = 0;
            foreach (var item in result.Items)
            {
                runningBalance += (item.Debit - item.Credit);
                item.RunningBalance = runningBalance;
            }

            return result;
        }
    }
}
