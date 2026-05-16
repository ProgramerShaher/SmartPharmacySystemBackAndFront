using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

using SmartPharmacySystem.Application.IServices;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.Services
{
    public class CustomerReceiptService : ICustomerReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFinancialService _financialService;
        private readonly IJournalEntryService _journalEntryService;

        public CustomerReceiptService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialService financialService, IJournalEntryService journalEntryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _financialService = financialService;
            _journalEntryService = journalEntryService;
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

                // ==================== المحرك المحاسبي الاحترافي ====================
                var journalEntry = new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryDto
                {
                    EntryDate = receipt.ReceiptDate,
                    VoucherNumber = $"REC-{receipt.Id}", // معرّف فريد للسند
                    Description = $"سند قبض من العميل: {customer.Name} - {dto.Notes}",
                    Type = SmartPharmacySystem.Core.Enums.VoucherType.ReceiptVoucher,
                    Lines = new List<SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto>()
                };

                // 1. الطرف المدين (من حـ/ الصندوق)
                journalEntry.Lines.Add(new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                {
                    AccountId = 1101, // الصندوق الرئيسي
                    Debit = dto.Amount,
                    Credit = 0,
                    Description = $"تحصيل مبلغ بموجب سند قبض رقم {receipt.Id}"
                });

                // 2. الطرف الدائن (إلى حـ/ العميل)
                journalEntry.Lines.Add(new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                {
                    AccountId = customer.AccountId ?? 1201, // حساب العميل الخاص أو ذمم العملاء
                    Debit = 0,
                    Credit = dto.Amount,
                    Description = $"سداد دفعة من الحساب - سند رقم {receipt.Id}"
                });

                // حفظ وترحيل القيد
                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                // الإبقاء على النظام القديم مؤقتاً
                await _financialService.ProcessTransactionAsync(
                    accountId: 1,
                    amount: dto.Amount,
                    type: FinancialTransactionType.Income,
                    referenceType: ReferenceType.CustomerReceipt,
                    referenceId: receipt.Id,
                    description: $"[نظام قديم] سند قبض: {customer.Name}. مرجع: {dto.ReferenceNo}");

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
