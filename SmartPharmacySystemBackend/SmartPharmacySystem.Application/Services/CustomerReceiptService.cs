using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class CustomerReceiptService : ICustomerReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFinancialService _financialService;

        public CustomerReceiptService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _financialService = financialService;
        }

        public async Task<CustomerReceiptDto> CreateAsync(CreateCustomerReceiptDto dto, int userId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException("العميل غير موجود");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Create Receipt Record
                var receipt = _mapper.Map<Core.Entities.CustomerReceipt>(dto);
                receipt.CreatedBy = userId;
                receipt.CreatedAt = DateTime.UtcNow;
                
                await _unitOfWork.CustomerReceipts.AddAsync(receipt);
                await _unitOfWork.SaveChangesAsync(); // Get ID first for linking

                // 2. Handle Linked Invoice (Mark as Paid)
                if (dto.SaleInvoiceId.HasValue)
                {
                    var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(dto.SaleInvoiceId.Value);
                    if (invoice != null && invoice.CustomerId == dto.CustomerId)
                    {
                        invoice.IsPaid = true;
                        await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                    }
                }

                // 3. Update Customer Balance (Decrease Debt)
                await _unitOfWork.Customers.UpdateBalanceAsync(customer.Id, -dto.Amount);

                // 4. Vault Integration (Process Financial Transaction)
                await _financialService.ProcessTransactionAsync(
                    accountId: 1,
                    amount: dto.Amount,
                    type: FinancialTransactionType.Income,
                    referenceType: ReferenceType.CustomerReceipt,
                    referenceId: receipt.Id,
                    description: $"سند قبض من العميل: {customer.Name}. {(string.IsNullOrEmpty(dto.ReferenceNo) ? "" : "مرجع: " + dto.ReferenceNo)} - {dto.Notes}");

                // 5. Commit
                await _unitOfWork.CommitAsync();

                var result = _mapper.Map<CustomerReceiptDto>(receipt);
                result.CustomerName = customer.Name;
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var receipt = await _unitOfWork.CustomerReceipts.GetByIdWithCustomerAsync(id)
                ?? throw new KeyNotFoundException("السند غير موجود");

            if (receipt.IsCancelled)
                throw new InvalidOperationException("السند ملغى مسبقاً");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Mark as Cancelled
                receipt.IsCancelled = true;
                receipt.CancelledAt = DateTime.UtcNow;
                receipt.CancelledBy = userId;
                await _unitOfWork.CustomerReceipts.UpdateAsync(receipt);

                // 2. Reverse Customer Balance (Increase Debt)
                await _unitOfWork.Customers.UpdateBalanceAsync(receipt.CustomerId, receipt.Amount);

                // 3. Reverse Vault (Expense to reverse previous income)
                await _financialService.ProcessTransactionAsync(
                    accountId: 1,
                    amount: receipt.Amount,
                    type: FinancialTransactionType.Expense,
                    referenceType: ReferenceType.CustomerReceipt,
                    referenceId: receipt.Id,
                    description: $"إلغاء سند قبض للعميل: {receipt.Customer.Name}. رقم السند: {receipt.Id}");

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CustomerReceiptDto>> GetRecentReceiptsAsync(int customerId)
        {
            var receipts = await _unitOfWork.CustomerReceipts.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<CustomerReceiptDto>>(receipts);
        }

        public async Task<Application.Wrappers.PagedResponse<CustomerReceiptDto>> GetPagedAsync(string? search, int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            var (items, totalCount) = await _unitOfWork.CustomerReceipts.GetPagedAsync(search, page, pageSize, fromDate, toDate);
            var dtos = _mapper.Map<IEnumerable<CustomerReceiptDto>>(items);
            return new Application.Wrappers.PagedResponse<CustomerReceiptDto>(dtos, totalCount, page, pageSize);
        }
        public async Task<ReceiptStatisticsDto> GetStatisticsAsync()
        {
            var (totalCount, totalAmount, todayAmount) = await _unitOfWork.CustomerReceipts.GetStatisticsAsync();
            return new ReceiptStatisticsDto
            {
                TotalReceipts = totalCount,
                TotalAmount = totalAmount,
                TodayAmount = todayAmount
            };
        }
    }
}
