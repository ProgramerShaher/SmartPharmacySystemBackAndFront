using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class SaleInvoiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SaleInvoiceService> logger,
        IStockMovementService stockMovementService,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IFinancialService financialService,
        IAlertService alertService,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : ISaleInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<SaleInvoiceService> _logger = logger;
        private readonly IStockMovementService _stockMovementService = stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = invoiceNumberGenerator;
        private readonly IFinancialService _financialService = financialService;
        private readonly IAlertService _alertService = alertService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<SaleInvoiceDto> CreateAsync(CreateSaleInvoiceDto dto, int userId)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new InvalidOperationException("لا يمكن إنشاء فاتورة بدون أصناف. يرجى إضافة صنف واحد على الأقل.");

            // التحقق من وجود اسم العميل (مطلوب للزبون الطيار)
            // Validate customer name is provided (required for walk-in customers)
            if (!dto.CustomerId.HasValue && string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new InvalidOperationException("يجب إدخال اسم العميل للزبون الطيار.");

            // قاعدة عمل: الزبون الطيار يجب أن يدفع نقداً فقط
            // Business Rule: Walk-in customers can only pay cash
            if (!dto.CustomerId.HasValue && dto.PaymentMethod == PaymentType.Credit)
                throw new InvalidOperationException("لا يمكن البيع بالآجل إلا لعميل مسجل في النظام.");

            var entity = _mapper.Map<SaleInvoice>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = userId;
            entity.Status = DocumentStatus.Draft;

            if (entity.InvoiceDate == default)
                entity.InvoiceDate = DateTime.Today;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await ProcessFEFOAndFinancialsAsync(entity);

                entity.SaleInvoiceNumber = await _invoiceNumberGenerator.GenerateSaleInvoiceNumberAsync();
                await _unitOfWork.SaleInvoices.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
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
                    var detail = _mapper.Map<SaleInvoiceDetail>(itemDto);
                    detail.SaleInvoiceId = id;
                    entity.SaleInvoiceDetails.Add(detail);
                }

                await ProcessFEFOAndFinancialsAsync(entity);

                await _unitOfWork.SaleInvoices.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error updating sale invoice {Id}", id);
                throw;
            }
        }

        public async Task ApproveAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("الفاتورة بالفعل معتمدة أو ملغاة.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // قاعدة عمل: الزبون الطيار يجب أن يدفع نقداً فقط
                // Business Rule: Walk-in customers can only pay cash
                if (!invoice.CustomerId.HasValue && invoice.PaymentMethod == PaymentType.Credit)
                    throw new InvalidOperationException("لا يمكن البيع بالآجل إلا لعميل مسجل في النظام.");

                // التحقق من وجود اسم العميل للزبون الطيار
                // Validate customer name for walk-in customers
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

                // ✅ Optimized: Fetch all batches at once instead of one by one
                var batchIds = invoice.SaleInvoiceDetails.Select(d => d.BatchId).Distinct().ToList();
                var batches = await _unitOfWork.MedicineBatches.GetByIdsAsync(batchIds);
                var batchDict = batches.ToDictionary(b => b.Id);

                // Validate all batches first
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

                // ✅ Optimized: Update all batches in memory, then save once
                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    var batch = batchDict[detail.BatchId];
                    batch.RemainingQuantity -= detail.Quantity;
                    batch.SoldQuantity += detail.Quantity;
                    await _unitOfWork.MedicineBatches.UpdateAsync(batch);

                    // Initialize remaining quantity to return
                    detail.RemainingQtyToReturn = detail.Quantity;
                }

                if (invoice.PaymentMethod == PaymentType.Cash)
                {
                    await _financialService.ProcessTransactionAsync(
                        accountId: 1,
                        amount: invoice.TotalAmount,
                        type: FinancialTransactionType.Income,
                        referenceType: ReferenceType.SaleInvoice,
                        referenceId: invoice.Id,
                        description: $"بيع نقدي - فاتورة رقم: {invoice.SaleInvoiceNumber}"
                    );
                    invoice.IsPaid = true;
                }
                else
                {
                    invoice.IsPaid = false;
                    if (invoice.CustomerId.HasValue)
                    {
                        await _unitOfWork.Customers.UpdateBalanceAsync(invoice.CustomerId.Value, invoice.TotalAmount);
                    }
                }

                await _unitOfWork.SaleInvoices.UpdateAsync(invoice);
                await _stockMovementService.ProcessDocumentMovementsAsync(id, ReferenceType.SaleInvoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                // ✅ Optimized: Move alerts to background (fire and forget)
                // This prevents blocking the main transaction
                var medicineIds = invoice.SaleInvoiceDetails.Select(d => d.MedicineId).Distinct().ToList();
                _ = Task.Run(async () =>
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
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error approving sale invoice {Id}", id);
                throw;
            }
        }

        public async Task UnapproveSalesInvoiceAsync(int id)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (invoice.Status != DocumentStatus.Approved)
                throw new InvalidOperationException("الفاتورة ليست في حالة اعتماد لتتمكن من إلغاء الاعتماد.");

            var associatedReturns = await _unitOfWork.SalesReturns.GetBySaleInvoiceIdAsync(id);
            if (associatedReturns.Any(r => r.Status != DocumentStatus.Cancelled))
            {
                throw new InvalidOperationException("لا يمكن إلغاء اعتماد الفاتورة الأصلية لوجود مردودات مرتبطة بها. يرجى إلغاء المردودات أولاً.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var detail in invoice.SaleInvoiceDetails)
                {
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                    if (batch != null)
                    {
                        batch.RemainingQuantity += detail.Quantity;
                        batch.SoldQuantity -= detail.Quantity;
                        await _unitOfWork.MedicineBatches.UpdateAsync(batch);
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

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error unapproving sale invoice {Id}", id);
                throw;
            }
        }

        public async Task CancelAsync(int id, int userId)
        {
            var invoice = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (invoice.Status == DocumentStatus.Cancelled)
                throw new InvalidOperationException("الفاتورة ملغاة بالفعل.");

            var associatedReturns = await _unitOfWork.SalesReturns.GetBySaleInvoiceIdAsync(id);
            if (associatedReturns.Any(r => r.Status != DocumentStatus.Cancelled))
            {
                throw new InvalidOperationException("لا يمكن إلغاء الفاتورة الأصلية لوجود مردودات مرتبطة بها. يرجى إلغاء المردودات أولاً.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wasApproved = invoice.Status == DocumentStatus.Approved;
                invoice.Status = DocumentStatus.Cancelled;
                invoice.CancelledBy = userId;
                invoice.CancelledAt = DateTime.UtcNow;

                if (wasApproved)
                {
                    foreach (var detail in invoice.SaleInvoiceDetails)
                    {
                        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId);
                        if (batch != null)
                        {
                            batch.RemainingQuantity += detail.Quantity;
                            batch.SoldQuantity -= detail.Quantity;
                            await _unitOfWork.MedicineBatches.UpdateAsync(batch);
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
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sale invoice {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");

            if (entity.Status != DocumentStatus.Draft)
                throw new InvalidOperationException("لا يمكن حذف فاتورة تم اعتمادها. يجب إلغاؤها بدلاً من ذلك.");

            await _unitOfWork.SaleInvoices.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SaleInvoiceDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.SaleInvoices.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"فاتورة المبيعات برقم {id} غير موجودة");
            return _mapper.Map<SaleInvoiceDto>(entity);
        }

        public async Task<IEnumerable<SaleInvoiceDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.SaleInvoices.GetAllAsync();
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

            await _unitOfWork.BeginTransactionAsync();
            try
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
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error receiving payment for credit sale invoice {InvoiceId}", invoiceId);
                throw;
            }
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
                int remainingToAllocate = detail.Quantity;

                if (detail.BatchId > 0)
                {
                    var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(detail.BatchId)
                        ?? throw new KeyNotFoundException($"التشغيلة {detail.BatchId} غير موجودة");

                    int canTake = Math.Min(remainingToAllocate, batch.RemainingQuantity);
                    if (canTake > 0)
                    {
                        var splitDetail = CreateSplitDetail(detail, batch, canTake);
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
                            var splitDetail = CreateSplitDetail(detail, batch, canTake);
                            invoice.SaleInvoiceDetails.Add(splitDetail);
                            remainingToAllocate -= canTake;
                        }
                    }
                }

                if (remainingToAllocate > 0)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedicineId);
                    throw new InvalidOperationException($"الكمية المطلوبة من الصنف ({medicine?.Name ?? detail.MedicineId.ToString()}) غير متوفرة في المخزن. العجز: {remainingToAllocate}");
                }
            }

            invoice.TotalAmount = invoice.SaleInvoiceDetails.Sum(d => d.TotalLineAmount);
            invoice.TotalCost = invoice.SaleInvoiceDetails.Sum(d => d.TotalCost);
            invoice.TotalProfit = invoice.SaleInvoiceDetails.Sum(d => d.Profit);
        }

        private SaleInvoiceDetail CreateSplitDetail(SaleInvoiceDetail template, MedicineBatch batch, int quantity)
        {
            return new SaleInvoiceDetail
            {
                MedicineId = template.MedicineId,
                BatchId = batch.Id,
                Quantity = quantity,
                SalePrice = template.SalePrice,
                UnitCost = batch.UnitPurchasePrice,
                TotalCost = quantity * batch.UnitPurchasePrice,
                TotalLineAmount = quantity * template.SalePrice,
                Profit = (quantity * template.SalePrice) - (quantity * batch.UnitPurchasePrice)
            };
        }
    }
}
