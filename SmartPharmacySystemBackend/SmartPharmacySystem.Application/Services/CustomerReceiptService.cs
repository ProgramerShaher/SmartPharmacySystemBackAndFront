using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly IWhatsAppNotificationService _whatsappNotificationService;
        private readonly ILogger<CustomerReceiptService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IClosingValidationService _closingValidationService;

        public CustomerReceiptService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialService financialService, IJournalEntryService journalEntryService, IWhatsAppNotificationService whatsappNotificationService, ILogger<CustomerReceiptService> logger, IServiceScopeFactory serviceScopeFactory, IClosingValidationService closingValidationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _financialService = financialService;
            _journalEntryService = journalEntryService;
            _whatsappNotificationService = whatsappNotificationService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _closingValidationService = closingValidationService;
        }

        public async Task<CustomerReceiptDto> CreateAsync(CreateCustomerReceiptDto dto, int userId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException("العميل غير موجود");

            if (customer.Balance <= 0)
                throw new InvalidOperationException("لا يمكن إنشاء سند قبض لأن رصيد العميل الحالي صفر أو لا يوجد عليه مديونية.");

            if (dto.Amount > customer.Balance)
                throw new InvalidOperationException($"لا يمكن تسديد مبلغ ({dto.Amount:N0}) أكبر من المديونية المتبقية على العميل ({customer.Balance:N0}).");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Create Receipt Record
                var receipt = _mapper.Map<Core.Entities.CustomerReceipt>(dto);
                receipt.CreatedBy = userId;
                receipt.CreatedAt = DateTime.UtcNow;
                
                await _closingValidationService.ValidateDateIsUnlockedAsync(receipt.ReceiptDate);
                
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

                var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
                
                var cashAccount = await _unitOfWork.Accounts.GetByCodeAsync($"11101-{userId}")
                                  ?? await _unitOfWork.Accounts.GetByCodeAsync("11101") 
                                  ?? allAccounts.FirstOrDefault(a => a.Name.Contains("صندوق") || a.Name.Contains("نقد")) 
                                  ?? allAccounts.FirstOrDefault() 
                                  ?? throw new InvalidOperationException("حساب الصندوق غير موجود، يرجى تهيئة دليل الحسابات أولاً.");
                                  
                var receivablesAccount = await _unitOfWork.Accounts.GetByCodeAsync("112") 
                                         ?? allAccounts.FirstOrDefault(a => a.Name.Contains("ذمم") || a.Name.Contains("عملاء")) 
                                         ?? allAccounts.FirstOrDefault() 
                                         ?? throw new InvalidOperationException("حساب الذمم المدينة غير موجود");

                // 1. الطرف المدين (من حـ/ الصندوق)
                journalEntry.Lines.Add(new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                {
                    AccountId = cashAccount.Id, // الصندوق الرئيسي
                    Debit = dto.Amount,
                    Credit = 0,
                    Description = $"تحصيل مبلغ بموجب سند قبض رقم {receipt.Id}"
                });

                // 2. الطرف الدائن (إلى حـ/ العميل)
                journalEntry.Lines.Add(new SmartPharmacySystem.Application.DTOs.Financial.JournalEntryLineDto
                {
                    AccountId = customer.AccountId ?? receivablesAccount.Id, // حساب العميل الخاص أو ذمم العملاء
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

                // ==================== إشعارات الواتساب (WhatsApp Notifications) ====================
                string wCustomerName = string.IsNullOrWhiteSpace(customer.Name) ? "عميلنا العزيز" : customer.Name;
                string wPhoneNumber = customer.PhoneNumber ?? string.Empty;
                int wCustomerId = customer.Id;
                decimal wAmount = dto.Amount;
                int wReceiptId = receipt.Id;
                DateTime wReceiptDate = receipt.ReceiptDate;
                int? wSaleInvoiceId = dto.SaleInvoiceId;
                decimal wBalanceBefore = customer.Balance;

                _logger.LogInformation("WhatsApp RECEIPT: Queuing notification for CustomerId={CustomerId}, Name={Name}, Phone={Phone}, Amount={Amount}",
                    wCustomerId, wCustomerName, wPhoneNumber, wAmount);

                Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("WhatsApp RECEIPT: Background task started for ReceiptId={ReceiptId}", wReceiptId);

                        if (string.IsNullOrWhiteSpace(wPhoneNumber))
                        {
                            _logger.LogWarning("WhatsApp RECEIPT: Skipped - empty phone number for CustomerId={CustomerId}", wCustomerId);
                            return;
                        }

                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var scopedUoW = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            var scopedWhatsApp = scope.ServiceProvider.GetRequiredService<IWhatsAppNotificationService>();
                            var scopedConfig = scope.ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
                            bool includeItems = true;
                            try
                            {
                                var includeSetting = scopedConfig?["WhatsAppSettings:IncludeItemsInMessage"];
                                if (!string.IsNullOrWhiteSpace(includeSetting) && bool.TryParse(includeSetting, out var parsed))
                                    includeItems = parsed;
                                _logger.LogInformation("WhatsApp RECEIPT: IncludeItemsInMessage setting = {Val}", includeItems);
                            }
                            catch { includeItems = true; }

                            var updatedCust = await scopedUoW.Customers.GetByIdAsync(wCustomerId);
                            decimal currentBalance = updatedCust?.Balance ?? (wBalanceBefore - wAmount);
                            if (currentBalance < 0) currentBalance = 0;

                            _logger.LogInformation("WhatsApp RECEIPT: Customer latest balance = {Balance}, Phone = {Phone}",
                                currentBalance, wPhoneNumber);

                            string safeCustomerName = wCustomerName;
                            safeCustomerName = System.Text.RegularExpressions.Regex.Replace(safeCustomerName, @"[\r\n\t\u202A-\u202E\u200E\u200F]", " ").Trim();
                            if (safeCustomerName.Length > 60) safeCustomerName = safeCustomerName.Substring(0, 60);

                            string itemsBlock = string.Empty;
                            int itemsCount = 0;

                            if (includeItems && wSaleInvoiceId.HasValue)
                            {
                                try
                                {
                                    _logger.LogInformation("WhatsApp RECEIPT: Loading linked invoice #{Inv} for items...", wSaleInvoiceId.Value);
                                    var linkedInv = await scopedUoW.SaleInvoices.GetByIdForDisplayAsync(wSaleInvoiceId.Value);
                                    if (linkedInv != null && linkedInv.SaleInvoiceDetails != null && linkedInv.SaleInvoiceDetails.Count > 0)
                                    {
                                        var details = linkedInv.SaleInvoiceDetails
                                            .Where(x => x != null)
                                            .OrderBy(x => x.Id)
                                            .Take(20)
                                            .ToList();

                                        itemsCount = linkedInv.SaleInvoiceDetails.Count;
                                        var sb = new System.Text.StringBuilder();
                                        sb.AppendLine();
                                        sb.AppendLine("--- تفاصيل الأصناف المسددة ---");
                                        int idx = 1;
                                        foreach (var d in details)
                                        {
                                            try
                                            {
                                                string medName = $"صنف #{d.MedicineId}";
                                                try
                                                {
                                                    if (d.Medicine != null && !string.IsNullOrWhiteSpace(d.Medicine.Name))
                                                    {
                                                        medName = d.Medicine.Name;
                                                        medName = System.Text.RegularExpressions.Regex.Replace(medName, @"[\r\n\t\u202A-\u202E\u200E\u200F]", " ").Trim();
                                                        if (medName.Length > 50) medName = medName.Substring(0, 50);
                                                    }
                                                }
                                                catch { }

                                                try
                                                {
                                                    if (d.Medicine != null && !string.IsNullOrWhiteSpace(d.Medicine.ScientificName) && d.Medicine.ScientificName != medName)
                                                    {
                                                        string sciName = d.Medicine.ScientificName!;
                                                        sciName = System.Text.RegularExpressions.Regex.Replace(sciName, @"[\r\n\t\u202A-\u202E\u200E\u200F]", " ").Trim();
                                                        if (sciName.Length > 40) sciName = sciName.Substring(0, 40);
                                                        medName = $"{medName} ({sciName})";
                                                    }
                                                }
                                                catch { }

                                                string unitName = "حبة";
                                                try
                                                {
                                                    if (d.SaleUnit != null && !string.IsNullOrWhiteSpace(d.SaleUnit.Name))
                                                    {
                                                        unitName = d.SaleUnit.Name;
                                                    }
                                                    else if (d.Medicine != null && !string.IsNullOrWhiteSpace(d.Medicine.BaseUnitName))
                                                    {
                                                        unitName = d.Medicine.BaseUnitName;
                                                    }
                                                    unitName = System.Text.RegularExpressions.Regex.Replace(unitName, @"[\r\n\t]", " ").Trim();
                                                    if (unitName.Length > 15) unitName = unitName.Substring(0, 15);
                                                }
                                                catch { unitName = "حبة"; }

                                                int qtyToShow = d.QuantityInSaleUnit > 0 ? d.QuantityInSaleUnit : d.Quantity;
                                                if (qtyToShow < 0) qtyToShow = 0;

                                                decimal lineTotal = d.TotalLineAmount;
                                                if (lineTotal < 0) lineTotal = 0;

                                                if (sb.Length > 3500)
                                                {
                                                    sb.AppendLine($"... (تم اقتطاع باقي الأصناف)");
                                                    break;
                                                }

                                                sb.AppendLine($"{idx}. {medName} × {qtyToShow} {unitName} = {lineTotal:N0} ريال يمني");
                                                idx++;
                                            }
                                            catch (Exception itemEx)
                                            {
                                                _logger.LogWarning(itemEx, "WhatsApp RECEIPT: Skipping bad item line #{ItemIdx}", idx);
                                                continue;
                                            }
                                        }
                                        if (itemsCount > 20)
                                            sb.AppendLine($"... (و{itemsCount - 20} صنفاً آخر - إجمالي {itemsCount})");
                                        sb.AppendLine("------------------------");
                                        itemsBlock = sb.ToString();
                                        if (itemsBlock.Length > 4000)
                                            itemsBlock = itemsBlock.Substring(0, 4000) + "\n... (تم اقتطاع النص)";
                                    }
                                    else
                                    {
                                        _logger.LogWarning("WhatsApp RECEIPT: No details for linked invoice Inv={Inv} (linkedInv={Null1}, Details={Null2}, Count={Cnt})",
                                            wSaleInvoiceId.Value, linkedInv == null, linkedInv?.SaleInvoiceDetails == null,
                                            linkedInv?.SaleInvoiceDetails?.Count ?? 0);
                                    }
                                    _logger.LogInformation("WhatsApp RECEIPT: Items block OK. Items={Items}, BlockLen={Len}", itemsCount, itemsBlock.Length);
                                }
                                catch (Exception itemsEx)
                                {
                                    _logger.LogWarning(itemsEx, "⚠️  WhatsApp RECEIPT: FAILED to build items list (sending simple message instead). Error: {Msg}", itemsEx.Message);
                                    itemsBlock = string.Empty;
                                }
                            }

                            string msg = $"مرحباً {safeCustomerName}،\nتم استلام مبلغ {wAmount:N0} ريال يمني بموجب سند قبض رقم {wReceiptId} بتاريخ {wReceiptDate:yyyy-MM-dd}.";

                            if (wSaleInvoiceId.HasValue)
                            {
                                msg += $"\nالسداد يخص فاتورة مبيعات رقم {wSaleInvoiceId.Value}.";
                            }

                            msg += $"\n{itemsBlock}";
                            msg += $"إجمالي المبلغ المتبقي عليك: {currentBalance:N0} ريال يمني.\nشكراً لتعاملكم معنا.";

                            if (msg.Length > 5000)
                            {
                                _logger.LogWarning("WhatsApp RECEIPT: Message too long ({Len}), trimming and using simple message...", msg.Length);
                                msg = $"مرحباً {safeCustomerName}،\nتم استلام مبلغ {wAmount:N0} ريال يمني بموجب سند قبض رقم {wReceiptId} بتاريخ {wReceiptDate:yyyy-MM-dd}.";
                                if (wSaleInvoiceId.HasValue)
                                    msg += $" فاتورة رقم {wSaleInvoiceId.Value}.";
                                    msg += $" عدد الأصناف: {itemsCount}. إجمالي المبلغ المتبقي عليك: {currentBalance:N0} ريال يمني. شكراً.";
                            }

                            _logger.LogInformation("WhatsApp RECEIPT: Message prepared. Items={Items}, Length={Len}. Sending now...", itemsCount, msg.Length);

                            bool sent = false;
                            try
                            {
                                sent = await scopedWhatsApp.SendMessageAsync(wPhoneNumber, msg);
                            }
                            catch (Exception sendEx)
                            {
                                _logger.LogError(sendEx, "WhatsApp RECEIPT: SendMessageAsync THREW exception, trying SIMPLE fallback message...");
                                try
                                {
                                    string simpleMsg = $"مرحباً {safeCustomerName}، تم استلام {wAmount:N0} ريال يمني بسند قبض رقم {wReceiptId}. إجمالي المبلغ المتبقي عليك: {currentBalance:N0} ريال يمني. شكراً.";
                                    sent = await scopedWhatsApp.SendMessageAsync(wPhoneNumber, simpleMsg);
                                }
                                catch (Exception fallbackEx)
                                {
                                    _logger.LogCritical(fallbackEx, "WhatsApp RECEIPT: EVEN FALLBACK FAILED for ReceiptId={ReceiptId}", wReceiptId);
                                    sent = false;
                                }
                            }

                            if (sent)
                                _logger.LogInformation("✅ WhatsApp RECEIPT: Message sent successfully to {Phone} for ReceiptId={ReceiptId}", wPhoneNumber, wReceiptId);
                            else
                                _logger.LogError("❌ WhatsApp RECEIPT: SendMessageAsync returned FALSE for ReceiptId={ReceiptId}, Phone={Phone}", wReceiptId, wPhoneNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "WhatsApp RECEIPT: FATAL EXCEPTION in background task for ReceiptId={ReceiptId}, CustomerId={CustomerId}", wReceiptId, wCustomerId);
                    }
                }).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogCritical(t.Exception, "FATAL GUARD (Receipt WhatsApp): UNHANDLED background task exception OBSERVED. API process is protected.");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);

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
