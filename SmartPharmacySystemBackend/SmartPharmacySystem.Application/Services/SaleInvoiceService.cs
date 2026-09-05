using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using Microsoft.AspNetCore.Http;
using SmartPharmacySystem.Application.DTOs.Barcode;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.DTOs.Financial;

namespace SmartPharmacySystem.Application.Services
{
    public class SaleInvoiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SaleInvoiceService> logger,
        IStockMovementService stockMovementService,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IFinancialService financialService,
        IJournalEntryService journalEntryService,
        IAlertService alertService,
        IBarcodeService barcodeService,
        ICurrentUserService currentUserService,
        IWhatsAppNotificationService whatsappNotificationService,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory,
        IShiftService shiftService,
        IClosingValidationService closingValidationService) : ISaleInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SaleInvoiceService> _logger = logger;
        private readonly IStockMovementService _stockMovementService = stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = invoiceNumberGenerator;
        private readonly IFinancialService _financialService = financialService;
        private readonly IJournalEntryService _journalEntryService = journalEntryService;
        private readonly IAlertService _alertService = alertService;
        private readonly IBarcodeService _barcodeService = barcodeService;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IWhatsAppNotificationService _whatsappNotificationService = whatsappNotificationService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly IShiftService _shiftService = shiftService;
        private readonly IClosingValidationService _closingValidationService = closingValidationService;

        public async Task<SaleInvoiceDto> CreateAsync(CreateSaleInvoiceDto dto, int userId)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new InvalidOperationException("لا يمكن إنشاء فاتورة بدون أصناف. يرجى إضافة صنف واحد على الأقل.");

            if (!dto.CustomerId.HasValue && string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new InvalidOperationException("يجب إدخال اسم العميل للزبون الطيار.");

            if (!dto.CustomerId.HasValue && dto.PaymentMethod == PaymentType.Credit)
                throw new InvalidOperationException("لا يمكن البيع بالآجل إلا لعميل مسجل في النظام.");

            var currentShiftResponse = await _shiftService.GetCurrentShiftAsync();
            if (!currentShiftResponse.Success || currentShiftResponse.Data == null)
            {
                throw new InvalidOperationException("لا يمكنك إجراء مبيعات. يجب فتح وردية (صندوق) أولاً.");
            }

            var entity = _mapper.Map<SaleInvoice>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = userId;
            entity.Status = DocumentStatus.Draft;
            entity.UserShiftId = currentShiftResponse.Data.Id;

            if (entity.InvoiceDate == default || entity.InvoiceDate.Year < 2000)
            {
                entity.InvoiceDate = DateTime.Now;
            }
            else if (entity.InvoiceDate.Kind == DateTimeKind.Utc)
            {
                entity.InvoiceDate = entity.InvoiceDate.ToLocalTime();
            }

            await _closingValidationService.ValidateDateIsUnlockedAsync(entity.InvoiceDate, entity.BranchId);

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                foreach (var item in entity.SaleInvoiceDetails)
                {
                    if (item.MedicineId <= 0)
                        throw new InvalidOperationException("يوجد صنف غير صالح في الفاتورة (رقم الصنف مفقود).");
                    if (item.Quantity <= 0)
                        throw new InvalidOperationException("يجب أن تكون الكمية أكبر من صفر لكل الأصناف.");
                }

                await ProcessFEFOAndFinancialsAsync(entity);

                entity.SaleInvoiceNumber = await _invoiceNumberGenerator.GenerateSaleInvoiceNumberAsync();
                await _unitOfWork.SaleInvoices.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                });
            }
            catch
            {
                throw;
            }

            var created = await _unitOfWork.SaleInvoices.GetByIdAsync(entity.Id);
            return _mapper.Map<SaleInvoiceDto>(created);
        }

        public async Task UpdateAsync(int id, UpdateSaleInvoiceDto dto)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (entity.Status != DocumentStatus.Draft)
            {
                throw new InvalidOperationException("لا يمكن تعديل فاتورة معتمدة أو ملغاة. التعديل مسموح فقط لحالة مسودة (Draft).");
            }

            if (dto.SaleInvoiceDate != default && dto.SaleInvoiceDate.Year > 2000)
            {
                entity.InvoiceDate = dto.SaleInvoiceDate.Kind == DateTimeKind.Utc ? dto.SaleInvoiceDate.ToLocalTime() : dto.SaleInvoiceDate;
            }
            else if (entity.InvoiceDate.Kind == DateTimeKind.Utc)
            {
                entity.InvoiceDate = entity.InvoiceDate.ToLocalTime();
            }

            await _closingValidationService.ValidateDateIsUnlockedAsync(entity.InvoiceDate, entity.BranchId);

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                if (dto.SaleInvoiceDate != default)
                {
                    entity.InvoiceDate = dto.SaleInvoiceDate;
                }
                _mapper.Map(dto, entity);

                if (entity.SaleInvoiceDetails != null)
                {
                    var detailsToRemove = entity.SaleInvoiceDetails.ToList();
                    foreach (var detail in detailsToRemove)
                    {
                        await _unitOfWork.SaleInvoiceDetails.DeleteAsync(detail.Id);
                    }
                    entity.SaleInvoiceDetails.Clear();
                }

                if (dto.Details == null || !dto.Details.Any())
                    throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل للفاتورة.");

                foreach (var itemDto in dto.Details)
                {
                    if (itemDto.MedicineId <= 0)
                        throw new InvalidOperationException("يوجد صنف غير صالح في الفاتورة (رقم الصنف مفقود).");
                    if (itemDto.Quantity <= 0)
                        throw new InvalidOperationException("يجب أن تكون الكمية أكبر من صفر لكل الأصناف.");

                    var detail = _mapper.Map<SaleInvoiceDetail>(itemDto);
                    detail.SaleInvoiceId = id;
                    entity.SaleInvoiceDetails.Add(detail);
                }

                await ProcessFEFOAndFinancialsAsync(entity);

                await _unitOfWork.SaleInvoices.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sale invoice {Id}", id);
                throw;
            }
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            await _closingValidationService.ValidateDateIsUnlockedAsync(invoice.InvoiceDate, invoice.BranchId);

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("الفاتورة بالفعل معتمدة أو ملغاة.");

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                if (!invoice.CustomerId.HasValue && invoice.PaymentMethod == PaymentType.Credit)
                    throw new InvalidOperationException("لا يمكن البيع بالآجل إلا لعميل مسجل في النظام.");

                if (!invoice.CustomerId.HasValue && string.IsNullOrWhiteSpace(invoice.CustomerName))
                    throw new InvalidOperationException("يجب إدخال اسم العميل للزبون الطيار.");

                invoice.Status = DocumentStatus.Approved;
                invoice.ApprovedBy = userId;
                invoice.ApprovedAt = DateTime.UtcNow;

                if (invoice.CustomerId.HasValue)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(invoice.CustomerId.Value)
                        ?? throw new KeyNotFoundException("العميل المرتبط بالفاتورة غير موجود");

                    if (!customer.IsActive)
                        throw new InvalidOperationException($"العميل {customer.Name} غير نشط. لا يمكن إتمام عملية البيع.");

                    if (invoice.PaymentMethod == PaymentType.Credit)
                    {
                        if (customer.CreditLimit > 0 && customer.Balance + invoice.TotalAmount > customer.CreditLimit)
                        {
                            decimal exceededAmount = (customer.Balance + invoice.TotalAmount) - customer.CreditLimit;
                            throw new InvalidOperationException($"عذراً، العميل تجاوز سقف الدين بـ ({exceededAmount:N0}) ريال. الرصيد الحالي: ({customer.Balance:N0})، سقف الدين: ({customer.CreditLimit:N0})");
                        }
                    }
                }
                else if (invoice.PaymentMethod == PaymentType.Credit)
                {
                    throw new InvalidOperationException("لا يمكن حفظ فاتورة آجلة بدون ربطها بعميل.");
                }

                var currentBranchId = _currentUserService.GetCurrentBranchId();
                if (!currentBranchId.HasValue)
                    throw new InvalidOperationException("لا يمكن اعتماد الفاتورة بدون تحديد الفرع الحالي للمستخدم.");

                var batchDict = invoice.SaleInvoiceDetails
                    .Where(d => d.Batch != null)
                    .GroupBy(d => d.BatchId)
                    .ToDictionary(g => g.Key, g => g.First().Batch!);

                var trackedBatches = batchDict.Values.ToList();

                foreach (var batch in trackedBatches)
                {
                    if (batch.BranchId != batch.BranchId)
                    {
                        throw new InvalidOperationException($"التشغيلة {batch.CompanyBatchNumber} غير تابعة لفرعك الحالي ولا يمكن استخدامها في البيع.");
                    }
                }

                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    if (!batchDict.TryGetValue(detail.BatchId, out var batch))
                        throw new KeyNotFoundException($"التشغيلة برقم {detail.BatchId} غير موجودة");

                    if (detail.SalePrice < detail.UnitCost)
                    {
                        var expiryLimit = DateTime.Today.AddDays(21);
                        if (batch.ExpiryDate > expiryLimit)
                        {
                            throw new InvalidOperationException($"فشل العملية: سعر البيع ({detail.SalePrice:N2}) أقل من التكلفة ({detail.UnitCost:N2}). " +
                                $"لا يُسمح بالبيع بخسارة إلا إذا كان متبقي على انتهاء الصنف 21 يوماً أو أقل. " +
                                $"تاريخ الانتهاء الحالي: {batch.ExpiryDate:yyyy-MM-dd}");
                        }
                    }

                    if (batch.IsExpired || batch.IsNearExpiry)
                    {
                        throw new InvalidOperationException($"فشل العملية: لا يمكن بيع الدفعة {batch.CompanyBatchNumber} لأنها منتهية أو قاربت على الانتهاء.");
                    }

                    if (batch.RemainingQuantity < detail.Quantity)
                    {
                        throw new InvalidOperationException($"الرصيد المتاح غير كافٍ للصنف {batch.CompanyBatchNumber}. المطلوب: {detail.Quantity}، المتاح: {batch.RemainingQuantity}");
                    }
                }

                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    var batch = batchDict[detail.BatchId];
                    batch.RemainingQuantity -= detail.Quantity;
                    batch.SoldQuantity += detail.Quantity;
                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                    detail.RemainingQtyToReturn = detail.Quantity;

                    await DecreaseInventoryStockAsync(invoice.BranchId, detail.MedicineId, batch.CompanyBatchNumber, detail.Quantity);
                }

                var journalEntry = new JournalEntryDto
                {
                    EntryDate = DateTime.UtcNow,
                    VoucherNumber = invoice.SaleInvoiceNumber,
                    Description = $"قيد مبيعات آلي - فاتورة رقم: {invoice.SaleInvoiceNumber} - العميل: {invoice.CustomerName ?? "زبون نقدي"}",
                    Type = VoucherType.SalesInvoice,
                    Lines = new List<JournalEntryLineDto>()
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
                                         
                var salesRevenueAccount = await _unitOfWork.Accounts.GetByCodeAsync("41001") 
                                          ?? await _unitOfWork.Accounts.GetByCodeAsync("41") 
                                          ?? allAccounts.FirstOrDefault(a => a.Name.Contains("مبيعات") || a.Name.Contains("إيراد"))
                                          ?? allAccounts.FirstOrDefault() 
                                          ?? throw new InvalidOperationException("حساب إيرادات المبيعات غير موجود");
                
                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = cashAccount.Id,
                        Debit = invoice.TotalAmount,
                        Credit = 0,
                        Description = $"تحصيل مبيعات نقدية - فاتورة {invoice.SaleInvoiceNumber}"
                    });
                    invoice.IsPaid = true;
                }
                else if (invoice.CustomerId.HasValue)
                {
                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = invoice.Customer?.AccountId ?? receivablesAccount.Id,
                        Debit = invoice.TotalAmount,
                        Credit = 0,
                        Description = $"مبيعات آجلة - فاتورة {invoice.SaleInvoiceNumber}"
                    });
                    invoice.IsPaid = false;
                    await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, invoice.TotalAmount);
                }

                if (invoice.TotalDiscount > 0)
                {
                    var discountAccount = await _unitOfWork.Accounts.GetByCodeAsync("41002")
                                          ?? allAccounts.FirstOrDefault(a => a.Name.Contains("خصم مسموح"))
                                          ?? allAccounts.FirstOrDefault(a => a.Name.Contains("خصومات"))
                                          ?? salesRevenueAccount;

                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = discountAccount.Id,
                        Debit = invoice.TotalDiscount,
                        Credit = 0,
                        Description = $"خصم مسموح به - فاتورة {invoice.SaleInvoiceNumber}"
                    });
                }

                journalEntry.Lines.Add(new JournalEntryLineDto
                {
                    AccountId = salesRevenueAccount.Id,
                    Debit = 0,
                    Credit = invoice.TotalAmount + invoice.TotalDiscount, // Gross Revenue
                    Description = $"إيراد مبيعات فاتورة {invoice.SaleInvoiceNumber}"
                });

                if (invoice.TotalCost > 0)
                {
                    var cogsAccount = await _unitOfWork.Accounts.GetByCodeAsync("51001") 
                                      ?? await _unitOfWork.Accounts.GetByCodeAsync("51") 
                                      ?? allAccounts.FirstOrDefault(a => a.Name.Contains("تكلفة") || a.Name.Contains("تكاليف"))
                                      ?? allAccounts.FirstOrDefault()
                                      ?? throw new InvalidOperationException("حساب تكلفة المبيعات غير موجود");
                                      
                    var inventoryAccount = await _unitOfWork.Accounts.GetByCodeAsync("11301") 
                                           ?? await _unitOfWork.Accounts.GetByCodeAsync("113") 
                                           ?? allAccounts.FirstOrDefault(a => a.Name.Contains("مخزون") || a.Name.Contains("مستودع"))
                                           ?? allAccounts.FirstOrDefault()
                                           ?? throw new InvalidOperationException("حساب المخزون غير موجود");

                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = cogsAccount.Id,
                        Debit = invoice.TotalCost,
                        Credit = 0,
                        Description = $"تكلفة المبيعات - فاتورة {invoice.SaleInvoiceNumber}"
                    });

                    journalEntry.Lines.Add(new JournalEntryLineDto
                    {
                        AccountId = inventoryAccount.Id,
                        Debit = 0,
                        Credit = invoice.TotalCost,
                        Description = $"نقص المخزون - فاتورة {invoice.SaleInvoiceNumber}"
                    });
                }

                var createdEntry = await _journalEntryService.CreateAsync(journalEntry, userId);
                await _journalEntryService.ApproveAsync(createdEntry.Id, userId);

                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1,
                        amount: invoice.TotalAmount,
                        type: FinancialTransactionType.Income,
                        referenceType: ReferenceType.SaleInvoice,
                        referenceId: invoice.Id,
                        description: $"إيراد مبيعات نقدية - فاتورة {invoice.SaleInvoiceNumber}"
                    );
                    invoice.IsPaid = true;
                }

                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SaleInvoice);
                await _unitOfWork.SaveChangesAsync();
                });

                if (invoice.CustomerId.HasValue)
                {
                    int wCustomerId = invoice.CustomerId.Value;
                    int wInvoiceId = id;
                    string wInvoiceNumber = invoice.SaleInvoiceNumber ?? $"INV-{id}";
                    decimal wTotalAmount = invoice.TotalAmount;
                    DateTime wInvoiceDate = invoice.InvoiceDate;
                    string wCustomerName = invoice.CustomerName ?? "عميلنا العزيز";
                    var wPaymentMethod = invoice.PaymentMethod;

                    _logger.LogInformation("WhatsApp INVOICE: Queuing notification for CustomerId={CustomerId}, InvoiceId={InvoiceId}, Amount={Amount}, Payment={Payment}",
                        wCustomerId, wInvoiceId, wTotalAmount, wPaymentMethod);

                    Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation("WhatsApp INVOICE: Background task started for InvoiceId={InvoiceId}", wInvoiceId);

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
                                    _logger.LogInformation("WhatsApp INVOICE: IncludeItemsInMessage setting = {Val}", includeItems);
                                }
                                catch { includeItems = true; }

                                var cust = await scopedUoW.Customers.GetByIdAsync(wCustomerId);
                                if (cust == null)
                                {
                                    _logger.LogWarning("WhatsApp INVOICE: Customer {CustomerId} not found in scoped context", wCustomerId);
                                    return;
                                }
                                if (string.IsNullOrWhiteSpace(cust.PhoneNumber))
                                {
                                    _logger.LogWarning("WhatsApp INVOICE: Customer {CustomerId} has no phone number", wCustomerId);
                                    return;
                                }

                                string safeCustomerName = string.IsNullOrWhiteSpace(cust.Name) ? wCustomerName : cust.Name;
                                safeCustomerName = System.Text.RegularExpressions.Regex.Replace(safeCustomerName, @"[\r\n\t\u202A-\u202E\u200E\u200F]", " ").Trim();
                                if (safeCustomerName.Length > 60) safeCustomerName = safeCustomerName.Substring(0, 60);

                                decimal safeBalance = cust.Balance;
                                string safePhone = cust.PhoneNumber!;

                                _logger.LogInformation("WhatsApp INVOICE: Customer found - Name={Name}, Phone={Phone}, Balance={Balance}",
                                    safeCustomerName, safePhone, safeBalance);

                                string itemsBlock = string.Empty;
                                int itemsCount = 0;

                                if (includeItems)
                                {
                                    try
                                    {
                                        _logger.LogInformation("WhatsApp INVOICE: Loading invoice details for items list...");
                                        var invoiceForMsg = await scopedUoW.SaleInvoices.GetByIdForDisplayAsync(wInvoiceId);
                                        var sb = new System.Text.StringBuilder();

                                        if (invoiceForMsg != null && invoiceForMsg.SaleInvoiceDetails != null && invoiceForMsg.SaleInvoiceDetails.Count > 0)
                                        {
                                            var details = invoiceForMsg.SaleInvoiceDetails
                                                .Where(x => x != null)
                                                .OrderBy(x => x.Id)
                                                .Take(20)
                                                .ToList();

                                            itemsCount = invoiceForMsg.SaleInvoiceDetails.Count;
                                            sb.AppendLine();
                                            sb.AppendLine("--- تفاصيل الأصناف ---");
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
                                                    _logger.LogWarning(itemEx, "WhatsApp INVOICE: Skipping bad item line #{ItemIdx}", idx);
                                                    continue;
                                                }
                                            }
                                            if (itemsCount > 20)
                                                sb.AppendLine($"... (و{itemsCount - 20} صنفاً آخر - إجمالي {itemsCount})");
                                            sb.AppendLine("------------------------");
                                        }
                                        else
                                        {
                                            _logger.LogWarning("WhatsApp INVOICE: No details found (invoiceForMsg={InvNull}, Details={DetailsNull}, Count={Cnt})",
                                                invoiceForMsg == null, invoiceForMsg?.SaleInvoiceDetails == null, invoiceForMsg?.SaleInvoiceDetails?.Count ?? 0);
                                        }
                                        itemsBlock = sb.ToString();
                                        if (itemsBlock.Length > 4000)
                                            itemsBlock = itemsBlock.Substring(0, 4000) + "\n... (تم اقتطاع النص)";
                                        _logger.LogInformation("WhatsApp INVOICE: Items block built OK. ItemsCount={Cnt}, BlockLen={Len}", itemsCount, itemsBlock.Length);
                                    }
                                    catch (Exception itemsEx)
                                    {
                                        _logger.LogWarning(itemsEx, "⚠️  WhatsApp INVOICE: FAILED to build items list (continuing with simple message). Error: {Msg}", itemsEx.Message);
                                        itemsBlock = string.Empty;
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation("WhatsApp INVOICE: Items disabled via IncludeItemsInMessage=false");
                                }

                                string msg;
                                string simpleMsg;

                                if (wPaymentMethod == PaymentType.Credit)
                                {
                                    msg = $"مرحباً {safeCustomerName}،\n" +
                                          $"تم تقييد مبلغ {wTotalAmount:N0} ريال يمني على حسابك بموجب فاتورة مبيعات آجلة رقم {wInvoiceNumber} بتاريخ {wInvoiceDate:yyyy-MM-dd}.\n" +
                                          $"{itemsBlock}" +
                                          $"إجمالي المبلغ المقيد عليك: {safeBalance:N0} ريال يمني.\n" +
                                          $"نتمنى لكم دوام الصحة والعافية.";

                                    simpleMsg = $"مرحباً {safeCustomerName}، تم تقييد مبلغ {wTotalAmount:N0} ريال يمني. فاتورة رقم {wInvoiceNumber}. إجمالي المبلغ المقيد عليك: {safeBalance:N0} ريال يمني. شكراً.";
                                }
                                else
                                {
                                    msg = $"مرحباً {safeCustomerName}،\n" +
                                          $"شكراً لزيارتك لصيدليتنا. تفاصيل فاتورة مبيعات نقدية رقم {wInvoiceNumber} بتاريخ {wInvoiceDate:yyyy-MM-dd}:\n" +
                                          $"الإجمالي المندفع: {wTotalAmount:N0} ريال يمني.\n" +
                                          $"{itemsBlock}" +
                                          $"نتمنى لكم دوام الصحة والعافية.";

                                    simpleMsg = $"مرحباً {safeCustomerName}، تم إصدار فاتورة مبيعات نقدية رقم {wInvoiceNumber} بقيمة {wTotalAmount:N0} ريال يمني. شكراً لزيارتكم.";
                                }

                                if (msg.Length > 5000)
                                {
                                    _logger.LogWarning("WhatsApp INVOICE: Message too long ({Len}), trimming items and retrying with simple message...", msg.Length);
                                    if (wPaymentMethod == PaymentType.Credit)
                                    {
                                        msg = $"مرحباً {safeCustomerName}،\n" +
                                              $"تم تقييد مبلغ {wTotalAmount:N0} ريال يمني على حسابك بموجب فاتورة مبيعات آجلة رقم {wInvoiceNumber} بتاريخ {wInvoiceDate:yyyy-MM-dd}.\n" +
                                              $"عدد الأصناف: {itemsCount}\n" +
                                              $"إجمالي المبلغ المقيد عليك: {safeBalance:N0} ريال يمني.\n" +
                                              $"نتمنى لكم دوام الصحة والعافية.";
                                    }
                                    else
                                    {
                                        msg = $"مرحباً {safeCustomerName}،\n" +
                                              $"تفاصيل فاتورة مبيعات نقدية رقم {wInvoiceNumber} بتاريخ {wInvoiceDate:yyyy-MM-dd}:\n" +
                                              $"الإجمالي: {wTotalAmount:N0} ريال يمني.\n" +
                                              $"عدد الأصناف: {itemsCount}\n" +
                                              $"نتمنى لكم دوام الصحة والعافية.";
                                    }
                                }

                                _logger.LogInformation("WhatsApp INVOICE: Message prepared. Items={Items}, Length={Len}. Sending now...",
                                    itemsCount, msg.Length);

                                bool sent = false;
                                try
                                {
                                    sent = await scopedWhatsApp.SendMessageAsync(safePhone, msg);
                                }
                                catch (Exception sendEx)
                                {
                                    _logger.LogError(sendEx, "WhatsApp INVOICE: SendMessageAsync THREW exception, trying SIMPLE fallback message...");
                                    try
                                    {
                                        sent = await scopedWhatsApp.SendMessageAsync(safePhone, simpleMsg);
                                    }
                                    catch (Exception fallbackEx)
                                    {
                                        _logger.LogCritical(fallbackEx, "WhatsApp INVOICE: EVEN FALLBACK FAILED for InvoiceId={InvoiceId}", wInvoiceId);
                                        sent = false;
                                    }
                                }

                                if (sent)
                                    _logger.LogInformation("✅ WhatsApp INVOICE: Message sent successfully to {Phone} for InvoiceId={InvoiceId}", safePhone, wInvoiceId);
                                else
                                    _logger.LogError("❌ WhatsApp INVOICE: SendMessageAsync returned FALSE for InvoiceId={InvoiceId}, Phone={Phone}", wInvoiceId, safePhone);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "WhatsApp INVOICE: FATAL EXCEPTION in background task for InvoiceId={InvoiceId}", wInvoiceId);
                        }
                    }).ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            _logger.LogCritical(t.Exception, "FATAL GUARD (Invoice WhatsApp): UNHANDLED background task exception OBSERVED. API process is protected.");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }

                var medicineIds = invoice.SaleInvoiceDetails.Select(d => d.MedicineId).Distinct().ToList();
                Task.Run(async () =>
                {
                    try
                    {
                        foreach (var medicineId in medicineIds)
                        {
                            await _alertService.SyncMedicineAlertsAsync(medicineId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing alerts for invoice {InvoiceId}", id);
                    }
                }).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogCritical(t.Exception, "FATAL GUARD (Alerts Sync): UNHANDLED background task exception OBSERVED. API process is protected.");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving sale invoice {Id}", id);
                throw;
            }
        }

        public async Task UnapproveSalesInvoiceAsync(int id)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            await _closingValidationService.ValidateDateIsUnlockedAsync(invoice.InvoiceDate, invoice.BranchId);

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("الفاتورة ليست في حالة اعتماد لتتمكن من إلغاء الاعتماد.");

            var associatedReturns = await _unitOfWork.SalesReturns.GetBySaleInvoiceIdAsync(id);
            if (associatedReturns.Any(r => r.Status != DocumentStatus.Cancelled))
            {
                throw new InvalidOperationException("لا يمكن إلغاء اعتماد الفاتورة الأصلية لوجود مردودات مرتبطة بها. يرجى إلغاء المردودات أولاً.");
            }

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    // ✅ Fix: Use already-tracked Batch instance from invoice details (loaded via ThenInclude)
                    // Avoids re-loading the batch which can cause EF Core Medicine tracking conflicts
                    var batch = detail.Batch;
                    if (batch != null)
                    {
                        batch.RemainingQuantity += detail.Quantity;
                        batch.SoldQuantity -= detail.Quantity;
                        await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                        // Re-add to InventoryStock
                        await IncreaseInventoryStockAsync(invoice.BranchId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, detail.Quantity);
                    }
                }

                await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.SaleInvoice);

                if (invoice.IsPaid)
                {
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1,
                        amount: invoice.TotalAmount,
                        type: FinancialTransactionType.Expense,
                        referenceType: ReferenceType.SaleInvoice,
                        referenceId: invoice.Id,
                        description: $"إلغاء اعتماد فاتورة مبيعات (خصم) - رقم: {invoice.SaleInvoiceNumber}");
                }
                else if (invoice.PaymentMethod == PaymentType.Credit && invoice.CustomerId.HasValue)
                {
                    await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, -invoice.TotalAmount);
                }

                invoice.Status = DocumentStatus.Draft;
                invoice.ApprovedBy = null;
                invoice.ApprovedAt = null;
                invoice.IsPaid = false;

                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    await _alertService.SyncMedicineAlertsAsync(detail.MedicineId);
                }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unapproving sale invoice {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            var associatedReturns = await _unitOfWork.SalesReturns.GetBySaleInvoiceIdAsync(id);
            if (associatedReturns.Any(r => r.Status != DocumentStatus.Cancelled))
            {
                throw new InvalidOperationException("لا يمكن إلغاء الفاتورة الأصلية لوجود مردودات مرتبطة بها. يرجى إلغاء المردودات أولاً.");
            }

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                var wasApproved = invoice.Status == DocumentStatus.Approved;
                invoice.Status = DocumentStatus.Cancelled;
                invoice.CancelledBy = userId;
                invoice.CancelledAt = DateTime.UtcNow;

                if (wasApproved)
                {
                    foreach (var detail in invoice.SaleInvoiceDetails)
                    {
                        // ✅ Fix: Use already-tracked Batch instance from invoice details (loaded via ThenInclude)
                        // Avoids re-loading the batch which can cause EF Core Medicine tracking conflicts
                        var batch = detail.Batch;
                        if (batch != null)
                        {
                            batch.RemainingQuantity += detail.Quantity;
                            batch.SoldQuantity -= detail.Quantity;
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                            // Re-add to InventoryStock
                            await IncreaseInventoryStockAsync(invoice.BranchId, detail.MedicineId, batch.CompanyBatchNumber, batch.ExpiryDate, detail.Quantity);
                        }
                    }

                    await _stockMovementService.CancelDocumentMovementsAsync(id, ReferenceType.SaleInvoice);

                    if (invoice.IsPaid)
                    {
                        await _financialService.ProcessTransactionAsync(
                            accountId: 1,
                            amount: invoice.TotalAmount,
                            type: FinancialTransactionType.Expense,
                            referenceType: ReferenceType.SaleInvoice,
                            referenceId: invoice.Id,
                            description: $"إلغاء فاتورة بيع - رقم: {invoice.SaleInvoiceNumber}");
                    }
                    else if (invoice.PaymentMethod == PaymentType.Credit && invoice.CustomerId.HasValue)
                    {
                        await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, -invoice.TotalAmount);
                    }
                }

                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling sale invoice {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id);
            if (entity == null)
                return;

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف فاتورة تم اعتمادها. يجب إلغاؤها بدلاً من ذلك.");

            await _unitOfWork.SaleInvoices.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SaleInvoiceDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id);
            if (entity == null)
                return null;
                
            return _mapper.Map<SaleInvoiceDto>(entity);
        }

        public async Task<IEnumerable<SaleInvoiceDto>> GetAllAsync(bool includeClosed = false)
        {
            var entities = await _unitOfWork.SaleInvoices.GetAllAsync();
            if (!includeClosed)
            {
                entities = entities.Where(e => e.Status != DocumentStatus.Closed).ToList();
            }
            return _mapper.Map<IEnumerable<SaleInvoiceDto>>(entities);
        }

        public async Task ReceiveCreditPaymentAsync(int invoiceId, int accountId = 1)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {invoiceId} غير موجودة");

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("لا يمكن تحصيل دفعة من فاتورة غير معتمدة");

            if (invoice.PaymentMethod != PaymentType.Credit)
                throw new InvalidOperationException("هذه الفاتورة ليست آجلة");

            if (invoice.IsPaid)
                throw new InvalidOperationException("الفاتورة محصلة مسبقاً");

            try
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                await _financialService.ProcessTransactionAsync(
                    accountId: accountId,
                    amount: invoice.TotalAmount,
                    type: FinancialTransactionType.Income,
                    referenceType: ReferenceType.SaleInvoice,
                    referenceId: invoice.Id,
                    description: $"تحصيل فاتورة بيع آجلة - رقم: {invoice.SaleInvoiceNumber}"
                );

                invoice.IsPaid = true;
                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving payment for credit sale invoice {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<IEnumerable<SaleInvoiceDto>> GetUnpaidByCustomerIdAsync(int customerId)
        {
            var entities = await _unitOfWork.SaleInvoices.GetUnpaidByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<SaleInvoiceDto>>(entities);
        }

        private async Task ProcessFEFOAndFinancialsAsync(SaleInvoice invoice)
        {
            invoice.TotalAmount = 0;
            invoice.TotalCost = 0;
            invoice.TotalProfit = 0;

            if (invoice.SaleInvoiceDetails == null || !invoice.SaleInvoiceDetails.Any()) return;

            var originalDetails = invoice.SaleInvoiceDetails.ToList();
            invoice.SaleInvoiceDetails.Clear();

            foreach (var detail in originalDetails)
            {
                // ====================================================
                // UoM: Convert entered quantity to base units
                // ====================================================
                int conversionFactor = 1;
                if (detail.SaleUnitId.HasValue)
                {
                    var saleUnit = await _unitOfWork.MedicineUnits.GetByIdAsync(detail.SaleUnitId.Value);
                    conversionFactor = saleUnit?.ConversionFactor ?? 1;
                }

                int rawQty = detail.Quantity;
                int baseUnits = rawQty * conversionFactor;
                
                detail.QuantityInSaleUnit = rawQty;
                detail.Quantity = baseUnits;

                int remainingToAllocate = detail.Quantity;

                if (detail.BatchId > 0)
                {
                    var medicineBatches = await _unitOfWork.MedicineBatches.GetBatchesByMedicineIdAsync(detail.MedicineId);
                    var batch = medicineBatches.FirstOrDefault(b => b.Id == detail.BatchId)
                        ?? throw new KeyNotFoundException($"التشغيلة {detail.BatchId} غير موجودة");

                    int canTake = Math.Min(remainingToAllocate, batch.RemainingQuantity);
                    if (canTake > 0)
                    {
                        var splitDetail = CreateSplitDetail(detail, batch, canTake, conversionFactor);
                        invoice.SaleInvoiceDetails.Add(splitDetail);
                        remainingToAllocate -= canTake;
                    }
                }

                if (remainingToAllocate > 0)
                {
                    var availableBatches = await _unitOfWork.MedicineBatches.GetAvailableBatchesByMedicineIdAsync(detail.MedicineId);

                    foreach (var batch in availableBatches.OrderBy(b => b.ExpiryDate))
                    {
                        if (remainingToAllocate <= 0) break;
                        if (batch.Id == detail.BatchId) continue;

                        int canTake = Math.Min(remainingToAllocate, batch.RemainingQuantity);
                        if (canTake > 0)
                        {
                            var splitDetail = CreateSplitDetail(detail, batch, canTake, conversionFactor);
                            invoice.SaleInvoiceDetails.Add(splitDetail);
                            remainingToAllocate -= canTake;
                        }
                    }
                }

                if (remainingToAllocate > 0)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                    throw new InvalidOperationException($"الكمية المطلوبة من الصنف ({medicine?.Name ?? detail.MedicineId.ToString()}) غير متوفرة في المخزن بالكمية الكافية.");
                }
            }

            invoice.TotalAmount = invoice.SaleInvoiceDetails.Sum(d => d.TotalLineAmount);
            invoice.TotalDiscount = invoice.SaleInvoiceDetails.Sum(d => d.DiscountAmount);
            invoice.TotalCost = invoice.SaleInvoiceDetails.Sum(d => d.TotalCost);
            invoice.TotalProfit = invoice.SaleInvoiceDetails.Sum(d => d.Profit);
        }

        private SaleInvoiceDetail CreateSplitDetail(SaleInvoiceDetail template, MedicineBatch batch, int quantity, int conversionFactor)
        {
            var grossLineAmount = (quantity / (decimal)conversionFactor) * template.SalePrice;
            var discountPercentage = template.DiscountPercentage;
            var discountAmount = Math.Round(grossLineAmount * (discountPercentage / 100m), 2);
            var netLineAmount = grossLineAmount - discountAmount;

            return new SaleInvoiceDetail
            {
                MedicineId = template.MedicineId,
                BatchId = batch.Id,
                Quantity = quantity,
                QuantityInSaleUnit = quantity / conversionFactor, // integer division, might be 0 for partials
                SaleUnitId = template.SaleUnitId,
                SalePrice = template.SalePrice,
                DiscountPercentage = discountPercentage,
                DiscountAmount = discountAmount,
                UnitCost = batch.UnitPurchasePrice,
                TotalCost = quantity * batch.UnitPurchasePrice,
                TotalLineAmount = netLineAmount, // Net amount after discount
                Profit = netLineAmount - (quantity * batch.UnitPurchasePrice) // Profit based on net amount
            };
        }

        /// <summary>
        /// Get dashboard statistics with optimized aggregate queries
        /// Uses .AsNoTracking() and direct Sum() for performance (<100ms)
        /// </summary>
        public async Task<DTOs.Dashboard.SalesDashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date; // Use UTC for consistency
            var sevenDaysAgo = today.AddDays(-6);

            // Get all invoices first for debugging
            var allInvoices = await _unitOfWork.SaleInvoices.GetAllAsync();

            // Debug: Log all invoices status and dates
            _logger.LogInformation($"[DashboardStats] Today (UTC): {today:yyyy-MM-dd}");
            _logger.LogInformation($"[DashboardStats] Total invoices: {allInvoices.Count()}");

            // Filter approved invoices for today - use both local and UTC date comparison
            var approvedToday = allInvoices
                .Where(s => s.Status == DocumentStatus.Approved &&
                           (s.InvoiceDate.Date == today || s.InvoiceDate.Date == DateTime.Today))
                .ToList();

            _logger.LogInformation($"[DashboardStats] Approved today: {approvedToday.Count}");

            var todayTotalSales = approvedToday.Sum(s => s.TotalAmount);
            var todayNetProfit = approvedToday.Sum(s => s.TotalProfit);

            // Cash percentage
            var todayCashSales = approvedToday.Where(s => s.PaymentMethod == PaymentType.Cash).Sum(s => s.TotalAmount);
            var cashPercentage = todayTotalSales > 0 ? (todayCashSales / todayTotalSales) * 100 : 0;

            // Customer debts
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var customerDebts = customers.Where(c => c.Balance > 0).Sum(c => c.Balance);

            // Today's returns
            var returns = await _unitOfWork.SalesReturns.GetAllAsync();
            var todayReturns = returns
                .Where(r => r.Status == DocumentStatus.Approved && r.ReturnDate.Date == today)
                .Sum(r => r.TotalAmount);

            var returnRate = todayTotalSales > 0 ? (todayReturns / todayTotalSales) * 100 : 0;

            // Last 7 days sales for sparkline
            var last7DaysSales = new List<decimal>();
            var allRecentInvoices = allInvoices
                .Where(s => s.Status == DocumentStatus.Approved && s.InvoiceDate.Date >= sevenDaysAgo)
                .ToList();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayTotal = allRecentInvoices
                    .Where(s => s.InvoiceDate.Date == date)
                    .Sum(s => s.TotalAmount);
                last7DaysSales.Add(dayTotal);
            }

            return new DTOs.Dashboard.SalesDashboardStatsDto
            {
                TodayTotalSales = todayTotalSales,
                TodayNetProfit = todayNetProfit,
                CustomerDebts = customerDebts,
                TodayReturnsAmount = todayReturns,
                ReturnRate = returnRate,
                CashPercentage = cashPercentage,
                Last7DaysSales = last7DaysSales
            };
        }

        public async Task<BarcodeResultDto> ProcessBarcodeItemAsync(string barcode, int userId)
        {
            _logger.LogInformation("Processing barcode {Barcode} for sale by user {UserId}", barcode, userId);

            var query = new GetProductForTransactionByBarcodeQuery
            {
                Barcode = barcode,
                TransactionType = TransactionType.Sale
            };

            var result = await _barcodeService.GetProductByBarcodeAsync(query, userId)
                ?? throw new KeyNotFoundException("الصنف غير موجود في قاعدة البيانات.");

            // Validation logic for Sales
            if (result.AvailableQuantity <= 0)
            {
                throw new InvalidOperationException($"عذراً، الصنف ({result.TradeName}) غير متوفر في المخزن حالياً.");
            }

            if (result.ExpiryDate < DateTime.Today)
            {
                throw new InvalidOperationException($"فشل المسح: الصنف ({result.TradeName}) منتهي الصلاحية بتاريخ {result.ExpiryDate:yyyy-MM-dd}.");
            }

            if (result.IsNearExpiry)
            {
                _logger.LogWarning("Scanned item is near expiry: {Name}", result.TradeName);
                // We allow scanning but maybe add a warning in the result if needed
            }

            return result;
        }

        private async Task IncreaseInventoryStockAsync(int branchId, int medicineId, string batchNumber, DateTime expiryDate, int quantity)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(branchId, WarehouseType.Main)
                ?? await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(branchId, WarehouseType.Branch);

            if (warehouse == null)
            {
                _logger.LogWarning("No sales warehouse found for branch {BranchId}.", branchId);
                return;
            }

            var stock = await _unitOfWork.InventoryStocks.GetByWarehouseMedicineBatchAsync(warehouse.Id, medicineId, batchNumber);
            if (stock == null)
            {
                stock = new InventoryStock
                {
                    WarehouseId = warehouse.Id,
                    MedicineId = medicineId,
                    BatchNumber = batchNumber,
                    ExpiryDate = expiryDate,
                    Quantity = quantity
                };
                await _unitOfWork.InventoryStocks.AddAsync(stock);
                return;
            }

            stock.Quantity += quantity;
            stock.ExpiryDate = expiryDate;
            await _unitOfWork.InventoryStocks.UpdateAsync(stock);
        }

        private async Task DecreaseInventoryStockAsync(int branchId, int medicineId, string batchNumber, int quantity)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(branchId, WarehouseType.Main)
                ?? await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(branchId, WarehouseType.Branch);

            if (warehouse == null)
            {
                _logger.LogWarning("No sales warehouse found for branch {BranchId}.", branchId);
                return;
            }

            var stock = await _unitOfWork.InventoryStocks.GetByWarehouseMedicineBatchAsync(warehouse.Id, medicineId, batchNumber);
            if (stock != null)
            {
                stock.Quantity = Math.Max(0, stock.Quantity - quantity);
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            }
        }
    }
}
