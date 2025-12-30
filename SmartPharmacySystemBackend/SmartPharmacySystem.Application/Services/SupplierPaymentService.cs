using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SupplierPayments;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class SupplierPaymentService : ISupplierPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierPaymentService> _logger;
        private readonly IFinancialService _financialService;

        public SupplierPaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SupplierPaymentService> logger,
            IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _financialService = financialService;
        }

        public async Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentDto dto, int userId)
        {
            // 1. Validation: Check Vault Balance (Prevent negative cash flow if strictly enforced)
            // Assuming AccountId 1 is Main Vault.
            var vault = await _unitOfWork.Financials.GetAccountByIdAsync(1);
            if (vault == null) throw new InvalidOperationException("الخزينة الرئيسية غير موجودة.");

            if (vault.Balance < dto.Amount)
                throw new InvalidOperationException($"رصيد الخزينة ({vault.Balance}) غير كافٍ لإتمام عملية الدفع ({dto.Amount}).");

            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId)
                ?? throw new KeyNotFoundException("المورد غير موجود.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Create Payment Record
                var payment = new SupplierPayment
                {
                    SupplierId = dto.SupplierId,
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate,
                    ReferenceNo = dto.ReferenceNo,
                    Notes = dto.Notes,
                    CreatedBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.SupplierPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(); // Get ID

                // 3. Update Supplier Balance (Decrease Debt)
                supplier.Balance -= dto.Amount;
                await _unitOfWork.Suppliers.UpdateAsync(supplier);

                // 4. Financial Transaction (Expense from Vault)
                await _financialService.ProcessTransactionAsync(
                    accountId: 1, // Main Vault
                    amount: dto.Amount,
                    type: FinancialTransactionType.Expense,
                    referenceType: ReferenceType.SupplierPayment,
                    referenceId: payment.Id,
                    description: $"سند صرف لمورد: {supplier.Name} - {dto.Notes} (Ref: {dto.ReferenceNo})"
                );

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var resultDto = _mapper.Map<SupplierPaymentDto>(payment);
                resultDto.SupplierName = supplier.Name;
                return resultDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating supplier payment");
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var payment = await _unitOfWork.SupplierPayments.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("سند الصرف غير موجود.");

            if (payment.IsDeleted)
                throw new InvalidOperationException("السند ملغى بالفعل.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Logical Delete
                payment.IsDeleted = true;
                payment.DeletedAt = DateTime.UtcNow;
                payment.DeletedBy = userId;

                // 2. Reverse Supplier Balance (Increase Debt back)
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(payment.SupplierId);
                if (supplier != null)
                {
                    supplier.Balance += payment.Amount;
                    await _unitOfWork.Suppliers.UpdateAsync(supplier);
                }

                // 3. Reverse Financial Transaction (Income back to Vault)
                await _financialService.ProcessTransactionAsync(
                    accountId: 1,
                    amount: payment.Amount,
                    type: FinancialTransactionType.Income, // Money comes back
                    referenceType: ReferenceType.SupplierPayment,
                    referenceId: payment.Id,
                    description: $"إلغاء سند صرف رقم {payment.Id} - (استرداد لخزينة)"
                );

                await _unitOfWork.SupplierPayments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling supplier payment {Id}", id);
                throw;
            }
        }

        public async Task<SupplierStatementDto> GetStatementAsync(int supplierId)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId)
                ?? throw new KeyNotFoundException("المورد غير موجود.");

            // 1. Fetch Data
            // A. Purchase Invoices (Credit & Approved) -> Increase Debt (Credit in Statement, Debit in Accounting terms? Wait. Supplier View: We owe him. His Credit increases.)
            // Let's stick to simple Debt Logic in DTO: Credit (Debt Increase), Debit (Debt Decrease).
            var invoices = await _unitOfWork.PurchaseInvoices
                .GetAllAsync(); // TODO: Add predicate to repo or filter in memory if volume low. Better add repo method.

            var creditInvoices = invoices
                .Where(i => i.SupplierId == supplierId && i.PaymentMethod == PaymentType.Credit && i.Status == DocumentStatus.Approved)
                .Select(i => new StatementItemDto
                {
                    Date = i.PurchaseDate,
                    Type = "فاتورة شراء",
                    Reference = i.PurchaseInvoiceNumber ?? i.Id.ToString(),
                    Credit = i.TotalAmount, // We owe more
                    Debit = 0,
                    DocumentId = i.Id,
                    Notes = "شراء آجل"
                }).ToList();

            // B. Returns (Approved) -> Decrease Debt
            var returns = await _unitOfWork.PurchaseReturns.GetAllAsync();
            var supplierReturns = returns
                .Where(r => r.SupplierId == supplierId && r.Status == DocumentStatus.Approved)
                .Select(r => new StatementItemDto
                {
                    Date = r.ReturnDate,
                    Type = "مرتجع شراء",
                    Reference = r.Id.ToString(),
                    Credit = 0,
                    Debit = r.TotalAmount, // We owe less
                    DocumentId = r.Id,
                    Notes = "مرتجع بضاعة"
                }).ToList();

            // C. Payments (Not Deleted) -> Decrease Debt
            var payments = await _unitOfWork.SupplierPayments.GetAllAsync(); // Need repo method strictly for this
            var supplierPayments = payments
                .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
                .Select(p => new StatementItemDto
                {
                    Date = p.PaymentDate,
                    Type = "سند صرف",
                    Reference = p.ReferenceNo ?? p.Id.ToString(),
                    Credit = 0,
                    Debit = p.Amount, // We paid, so we owe less
                    DocumentId = p.Id,
                    Notes = p.Notes ?? ""
                }).ToList();


            // 2. Merge & Sort
            var allTransactions = creditInvoices
                .Concat(supplierReturns)
                .Concat(supplierPayments)
                .OrderBy(x => x.Date)
                .ToList();

            // 3. Calculate Running Balance
            decimal runningBalance = 0;
            foreach (var item in allTransactions)
            {
                // Balance = Previous + Credit (Purchase) - Debit (Payment/Return)
                runningBalance += item.Credit - item.Debit;
                item.RunningBalance = runningBalance;
            }

            // 4. Final Result
            return new SupplierStatementDto
            {
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                TotalBalance = runningBalance,
                Status = runningBalance == 0 ? "خالص" : "مديون",
                StatusColor = runningBalance == 0 ? "Green" : "Red",
                Transactions = allTransactions
            };
        }

        public async Task<IEnumerable<SupplierPaymentDto>> GetRecentPaymentsAsync(int supplierId = 0)
        {
            var payments = await _unitOfWork.SupplierPayments.GetAllAsync();
            if (supplierId > 0)
            {
                payments = payments.Where(p => p.SupplierId == supplierId);
            }

            var dtos = _mapper.Map<IEnumerable<SupplierPaymentDto>>(payments.Where(p => !p.IsDeleted).OrderByDescending(p => p.PaymentDate));

            // Enrich with Supplier Names manually if not included (Assuming Repo doesn't include by default)
            foreach (var dto in dtos)
            {
                // Fetch name optimized? Ideally use Include in Repo. For now:
                var sup = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId);
                dto.SupplierName = sup?.Name ?? "N/A";
            }
            return dtos;
        }
    }
}
