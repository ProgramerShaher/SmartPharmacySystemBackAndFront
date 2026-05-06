using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Reports;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// خدمة التقارير المركزية - مُحسَّنة للأداء
/// Central Reporting Service - Performance Optimized
/// All queries use .AsNoTracking() for Read-Only operations
/// </summary>
public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===================== كشف الحساب الموحد - Unified Statement =====================

    /// <inheritdoc/>
    public async Task<UnifiedStatementDto> GetUnifiedStatementAsync(
        string entityType,
        int entityId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        _logger.LogInformation("Generating unified statement for {EntityType} ID: {EntityId}", entityType, entityId);

        var result = new UnifiedStatementDto
        {
            EntityType = entityType,
            EntityId = entityId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (entityType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            await BuildCustomerStatementAsync(result, entityId, fromDate, toDate);
        }
        else if (entityType.Equals("Supplier", StringComparison.OrdinalIgnoreCase))
        {
            await BuildSupplierStatementAsync(result, entityId, fromDate, toDate);
        }
        else
        {
            throw new ArgumentException($"نوع الكيان غير معروف: {entityType}. يجب أن يكون Customer أو Supplier");
        }

        return result;
    }

    /// <summary>
    /// بناء كشف حساب العميل
    /// Build Customer Statement
    /// </summary>
    private async Task BuildCustomerStatementAsync(
        UnifiedStatementDto result,
        int customerId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        // 1. Get customer info - OPTIMIZED: Only select needed fields
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.Id, c.Name, c.PhoneNumber, c.Address, c.Balance })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"العميل برقم {customerId} غير موجود");

        result.EntityName = customer.Name;
        result.PhoneNumber = customer.PhoneNumber;
        result.Address = customer.Address;
        result.CurrentBalance = customer.Balance;

        var lines = new List<StatementLineDto>();

        // 2. Sale Invoices (Debit - Customer owes us)
        // OPTIMIZED: Direct projection without loading entities
        var invoiceLines = await _context.SaleInvoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId
                && i.Status == DocumentStatus.Approved
                && i.PaymentMethod == PaymentType.Credit
                && (!fromDate.HasValue || i.InvoiceDate >= fromDate.Value)
                && (!toDate.HasValue || i.InvoiceDate <= toDate.Value))
            .OrderBy(i => i.InvoiceDate)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new StatementLineDto
            {
                TransactionDate = i.InvoiceDate,
                ReferenceType = "فاتورة مبيعات",
                ReferenceNumber = i.SaleInvoiceNumber,
                ReferenceId = i.Id,
                Description = $"فاتورة مبيعات رقم {i.SaleInvoiceNumber}",
                Debit = i.TotalAmount,
                Credit = 0,
                CreatedByUserName = i.Creator != null ? i.Creator.FullName : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        lines.AddRange(invoiceLines);

        // 3. Customer Receipts (Credit - Customer paid us)
        var receiptLines = await _context.CustomerReceipts
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId
                && !r.IsCancelled
                && (!fromDate.HasValue || r.ReceiptDate >= fromDate.Value)
                && (!toDate.HasValue || r.ReceiptDate <= toDate.Value))
            .OrderBy(r => r.ReceiptDate)
            .ThenBy(r => r.CreatedAt)
            .Select(r => new StatementLineDto
            {
                TransactionDate = r.ReceiptDate,
                ReferenceType = "سند قبض",
                ReferenceNumber = r.ReferenceNo ?? $"REC-{r.Id}",
                ReferenceId = r.Id,
                Description = r.Notes ?? "تحصيل من العميل",
                Debit = 0,
                Credit = r.Amount,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        lines.AddRange(receiptLines);

        // 4. Sales Returns (Credit - We returned money to customer)
        var returnLines = await _context.SalesReturns
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId
                && r.Status == DocumentStatus.Approved
                && (!fromDate.HasValue || r.ReturnDate >= fromDate.Value)
                && (!toDate.HasValue || r.ReturnDate <= toDate.Value))
            .OrderBy(r => r.ReturnDate)
            .ThenBy(r => r.CreatedAt)
            .Select(r => new StatementLineDto
            {
                TransactionDate = r.ReturnDate,
                ReferenceType = "مرتجع مبيعات",
                ReferenceNumber = $"SR-{r.Id}",
                ReferenceId = r.Id,
                Description = r.Reason ?? "مرتجع مبيعات",
                Debit = 0,
                Credit = r.TotalAmount,
                CreatedByUserName = r.Creator != null ? r.Creator.FullName : null,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        lines.AddRange(returnLines);

        // 5. Sort all lines chronologically and calculate running balance
        var sortedLines = lines.OrderBy(l => l.TransactionDate).ThenBy(l => l.CreatedAt).ToList();

        // Calculate opening balance (balance before the from date)
        decimal openingBalance = 0;
        if (fromDate.HasValue)
        {
            // Get transactions before the from date
            var priorDebit = await _context.SaleInvoices
                .AsNoTracking()
                .Where(i => i.CustomerId == customerId
                    && i.Status == DocumentStatus.Approved
                    && i.PaymentMethod == PaymentType.Credit
                    && i.InvoiceDate < fromDate.Value)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var priorCredit = await _context.CustomerReceipts
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId && !r.IsCancelled && r.ReceiptDate < fromDate.Value)
                .SumAsync(r => (decimal?)r.Amount) ?? 0;

            var priorReturns = await _context.SalesReturns
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId && r.Status == DocumentStatus.Approved && r.ReturnDate < fromDate.Value)
                .SumAsync(r => (decimal?)r.TotalAmount) ?? 0;

            openingBalance = priorDebit - priorCredit - priorReturns;
        }

        result.OpeningBalance = openingBalance;

        // 6. Calculate running balance
        decimal runningBalance = openingBalance;
        foreach (var line in sortedLines)
        {
            runningBalance += (line.Debit - line.Credit);
            line.RunningBalance = runningBalance;
        }

        result.Lines = sortedLines;
        result.TotalDebit = sortedLines.Sum(l => l.Debit);
        result.TotalCredit = sortedLines.Sum(l => l.Credit);
    }

    /// <summary>
    /// بناء كشف حساب المورد
    /// Build Supplier Statement
    /// </summary>
    private async Task BuildSupplierStatementAsync(
        UnifiedStatementDto result,
        int supplierId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        // 1. Get supplier info
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == supplierId && !s.IsDeleted)
            .Select(s => new { s.Id, s.Name, s.PhoneNumber, s.Address, s.Balance })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"المورد برقم {supplierId} غير موجود");

        result.EntityName = supplier.Name;
        result.PhoneNumber = supplier.PhoneNumber;
        result.Address = supplier.Address;
        result.CurrentBalance = supplier.Balance;

        var lines = new List<StatementLineDto>();

        // 2. Purchase Invoices (Credit - We owe supplier)
        var invoiceLines = await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(i => i.SupplierId == supplierId
                && i.Status == DocumentStatus.Approved
                && i.PaymentMethod == PaymentType.Credit
                && (!fromDate.HasValue || i.PurchaseDate >= fromDate.Value)
                && (!toDate.HasValue || i.PurchaseDate <= toDate.Value))
            .OrderBy(i => i.PurchaseDate)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new StatementLineDto
            {
                TransactionDate = i.PurchaseDate,
                ReferenceType = "فاتورة شراء",
                ReferenceNumber = i.PurchaseInvoiceNumber,
                ReferenceId = i.Id,
                Description = $"فاتورة شراء رقم {i.PurchaseInvoiceNumber}",
                Debit = 0,
                Credit = i.TotalAmount,
                CreatedByUserName = i.Creator != null ? i.Creator.FullName : null,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        lines.AddRange(invoiceLines);

        // 3. Supplier Payments (Debit - We paid supplier)
        var paymentLines = await _context.SupplierPayments
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId
                && (!fromDate.HasValue || p.PaymentDate >= fromDate.Value)
                && (!toDate.HasValue || p.PaymentDate <= toDate.Value))
            .OrderBy(p => p.PaymentDate)
            .Select(p => new StatementLineDto
            {
                TransactionDate = p.PaymentDate,
                ReferenceType = "سند صرف",
                ReferenceNumber = p.ReferenceNo ?? $"SP-{p.Id}",
                ReferenceId = p.Id,
                Description = p.Notes ?? "دفعة للمورد",
                Debit = p.Amount,
                Credit = 0,
                CreatedAt = p.PaymentDate // SupplierPayment doesn't have CreatedAt
            })
            .ToListAsync();

        lines.AddRange(paymentLines);

        // 4. Purchase Returns (Debit - Supplier returns money to us)
        var returnLines = await _context.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.SupplierId == supplierId
                && r.Status == DocumentStatus.Approved
                && (!fromDate.HasValue || r.ReturnDate >= fromDate.Value)
                && (!toDate.HasValue || r.ReturnDate <= toDate.Value))
            .OrderBy(r => r.ReturnDate)
            .ThenBy(r => r.CreatedAt)
            .Select(r => new StatementLineDto
            {
                TransactionDate = r.ReturnDate,
                ReferenceType = "مرتجع مشتريات",
                ReferenceNumber = $"PR-{r.Id}",
                ReferenceId = r.Id,
                Description = r.Reason ?? "مرتجع مشتريات",
                Debit = r.TotalAmount,
                Credit = 0,
                CreatedByUserName = r.CreatedBy.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        lines.AddRange(returnLines);

        // 5. Sort and calculate running balance
        var sortedLines = lines.OrderBy(l => l.TransactionDate).ThenBy(l => l.CreatedAt).ToList();

        // Opening balance for supplier
        decimal openingBalance = 0;
        if (fromDate.HasValue)
        {
            var priorCredit = await _context.PurchaseInvoices
                .AsNoTracking()
                .Where(i => i.SupplierId == supplierId
                    && i.Status == DocumentStatus.Approved
                    && i.PaymentMethod == PaymentType.Credit
                    && i.PurchaseDate < fromDate.Value)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var priorDebit = await _context.SupplierPayments
                .AsNoTracking()
                .Where(p => p.SupplierId == supplierId && p.PaymentDate < fromDate.Value)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var priorReturns = await _context.PurchaseReturns
                .AsNoTracking()
                .Where(r => r.SupplierId == supplierId && r.Status == DocumentStatus.Approved && r.ReturnDate < fromDate.Value)
                .SumAsync(r => (decimal?)r.TotalAmount) ?? 0;

            openingBalance = priorCredit - priorDebit - priorReturns;
        }

        result.OpeningBalance = openingBalance;

        // For supplier: Credit increases balance (we owe more), Debit decreases it
        decimal runningBalance = openingBalance;
        foreach (var line in sortedLines)
        {
            runningBalance += (line.Credit - line.Debit);
            line.RunningBalance = runningBalance;
        }

        result.Lines = sortedLines;
        result.TotalDebit = sortedLines.Sum(l => l.Debit);
        result.TotalCredit = sortedLines.Sum(l => l.Credit);
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<StatementLineDto>> GetUnifiedStatementPagedAsync(
        string entityType,
        int entityId,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 50)
    {
        // Get full statement first (could optimize further with skip/take in queries)
        var fullStatement = await GetUnifiedStatementAsync(entityType, entityId, fromDate, toDate);

        var totalCount = fullStatement.Lines.Count;
        var pagedLines = fullStatement.Lines
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResponse<StatementLineDto>(pagedLines, totalCount, page, pageSize);
    }

    // ===================== تقرير صافي الأرباح - Net Profit Report =====================

    /// <inheritdoc/>
    public async Task<NetProfitReportDto> GetNetProfitReportAsync(
        DateTime fromDate,
        DateTime toDate,
        bool includeExpenseDetails = true)
    {
        _logger.LogInformation("Generating net profit report from {From} to {To}", fromDate, toDate);

        var result = new NetProfitReportDto
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        // 1. Gross Sales (approved invoices only)
        var salesData = await _context.SaleInvoices
            .AsNoTracking()
            .Where(i => i.Status == DocumentStatus.Approved
                && i.InvoiceDate >= fromDate
                && i.InvoiceDate <= toDate)
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalAmount = g.Sum(i => i.TotalAmount),
                TotalCost = g.Sum(i => i.TotalCost),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        result.GrossSales = salesData?.TotalAmount ?? 0;
        result.CostOfGoodsSold = salesData?.TotalCost ?? 0;
        result.SalesInvoiceCount = salesData?.Count ?? 0;

        // 2. Sales Returns
        result.SalesReturns = await _context.SalesReturns
            .AsNoTracking()
            .Where(r => r.Status == DocumentStatus.Approved
                && r.ReturnDate >= fromDate
                && r.ReturnDate <= toDate)
            .SumAsync(r => (decimal?)r.TotalAmount) ?? 0;

        // 3. Calculate discounts (if tracked separately, otherwise 0)
        result.SalesDiscounts = 0; // TODO: Implement if discounts are tracked

        // 4. Total Purchases (for reference)
        result.TotalPurchases = await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(i => i.Status == DocumentStatus.Approved
                && i.PurchaseDate >= fromDate
                && i.PurchaseDate <= toDate)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        // 5. Purchase Returns
        result.PurchaseReturns = await _context.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.Status == DocumentStatus.Approved
                && r.ReturnDate >= fromDate
                && r.ReturnDate <= toDate)
            .SumAsync(r => (decimal?)r.TotalAmount) ?? 0;

        // 6. Operating Expenses
        var expenseData = await _context.Expenses
            .AsNoTracking()
            .Where(e => !e.IsDeleted
                && e.ExpenseDate >= fromDate
                && e.ExpenseDate <= toDate)
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new ExpenseBreakdownDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Amount = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        result.TotalExpenses = expenseData.Sum(e => e.Amount);

        // Calculate percentages
        if (result.TotalExpenses > 0)
        {
            foreach (var expense in expenseData)
            {
                expense.Percentage = (double)Math.Round((expense.Amount / result.TotalExpenses) * 100, 2);
            }
        }

        if (includeExpenseDetails)
        {
            result.ExpensesByCategory = expenseData;
        }

        _logger.LogInformation("Net profit report generated: GrossSales={Gross}, COGS={COGS}, Expenses={Exp}, NetProfit={Net}",
            result.GrossSales, result.CostOfGoodsSold, result.TotalExpenses, result.NetProfit);

        return result;
    }

    // ===================== تقييم المخزون - Inventory Valuation =====================

    /// <inheritdoc/>
    public async Task<InventoryValuationDto> GetInventoryValuationAsync(InventoryValuationQueryDto query)
    {
        _logger.LogInformation("Generating inventory valuation with filter: {Filter}", query.ExpiryFilter);

        var result = new InventoryValuationDto();

        // Base query for batches
        var batchQuery = _context.MedicineBatches
            .AsNoTracking()
            .Where(b => !b.IsDeleted && b.RemainingQuantity > 0);

        // Apply expiry filter
        var today = DateTime.UtcNow.Date;
        batchQuery = query.ExpiryFilter?.ToLower() switch
        {
            "expired" => batchQuery.Where(b => b.ExpiryDate < today),
            "expiring" => batchQuery.Where(b => b.ExpiryDate >= today && b.ExpiryDate <= today.AddDays(30)),
            "active" => batchQuery.Where(b => b.ExpiryDate > today),
            _ => batchQuery
        };

        // Apply optional filters
        if (query.MedicineId.HasValue)
        {
            batchQuery = batchQuery.Where(b => b.MedicineId == query.MedicineId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            batchQuery = batchQuery.Where(b => b.Medicine.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            batchQuery = batchQuery.Where(b =>
                b.Medicine.Name.ToLower().Contains(searchTerm) ||
                (b.BatchBarcode != null && b.BatchBarcode.Contains(searchTerm)) ||
                b.CompanyBatchNumber.Contains(searchTerm));
        }

        // Get summary statistics first
        var summaryQuery = _context.MedicineBatches
            .AsNoTracking()
            .Where(b => !b.IsDeleted && b.RemainingQuantity > 0);

        result.TotalBatches = await summaryQuery.CountAsync();
        result.ActiveBatches = await summaryQuery.CountAsync(b => b.ExpiryDate > today);
        result.ExpiredBatches = await summaryQuery.CountAsync(b => b.ExpiryDate < today);
        result.ExpiringSoonBatches = await summaryQuery.CountAsync(b => b.ExpiryDate >= today && b.ExpiryDate <= today.AddDays(30));

        result.TotalCapital = await summaryQuery.SumAsync(b => (decimal?)b.RemainingQuantity * b.UnitPurchasePrice) ?? 0;
        result.TotalRetailValue = await summaryQuery.SumAsync(b => (decimal?)b.RemainingQuantity * b.RetailPrice) ?? 0;
        result.ExpiredStockValue = await summaryQuery
            .Where(b => b.ExpiryDate < today)
            .SumAsync(b => (decimal?)b.RemainingQuantity * b.UnitPurchasePrice) ?? 0;

        result.TotalQuantity = await summaryQuery.SumAsync(b => b.RemainingQuantity);

        // Apply sorting
        batchQuery = query.SortBy?.ToLower() switch
        {
            "capital" => query.SortDirection?.ToLower() == "desc"
                ? batchQuery.OrderByDescending(b => b.RemainingQuantity * b.UnitPurchasePrice)
                : batchQuery.OrderBy(b => b.RemainingQuantity * b.UnitPurchasePrice),
            "quantity" => query.SortDirection?.ToLower() == "desc"
                ? batchQuery.OrderByDescending(b => b.RemainingQuantity)
                : batchQuery.OrderBy(b => b.RemainingQuantity),
            _ => query.SortDirection?.ToLower() == "desc"
                ? batchQuery.OrderByDescending(b => b.ExpiryDate)
                : batchQuery.OrderBy(b => b.ExpiryDate)
        };

        // Get paginated batch details
        var batches = await batchQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BatchValuationDto
            {
                BatchId = b.Id,
                MedicineId = b.MedicineId,
                MedicineName = b.Medicine.Name,
                ScientificName = b.Medicine.ScientificName,
                CompanyBatchNumber = b.CompanyBatchNumber,
                BatchBarcode = b.BatchBarcode,
                ExpiryDate = b.ExpiryDate,
                RemainingQuantity = b.RemainingQuantity,
                UnitPurchasePrice = b.UnitPurchasePrice,
                RetailPrice = b.RetailPrice,
                PurchaseInvoiceId = b.PurchaseInvoiceId,
                EntryDate = b.EntryDate,
                StorageLocation = b.StorageLocation
            })
            .ToListAsync();

        result.Batches = batches;

        return result;
    }

    /// <inheritdoc/>
    public async Task<PagedResponse<BatchValuationDto>> GetInventoryValuationPagedAsync(InventoryValuationQueryDto query)
    {
        var fullResult = await GetInventoryValuationAsync(query);

        // Count total matching the filter
        var countQuery = _context.MedicineBatches
            .AsNoTracking()
            .Where(b => !b.IsDeleted && b.RemainingQuantity > 0);

        var today = DateTime.UtcNow.Date;
        countQuery = query.ExpiryFilter?.ToLower() switch
        {
            "expired" => countQuery.Where(b => b.ExpiryDate < today),
            "expiring" => countQuery.Where(b => b.ExpiryDate >= today && b.ExpiryDate <= today.AddDays(30)),
            "active" => countQuery.Where(b => b.ExpiryDate > today),
            _ => countQuery
        };

        if (query.MedicineId.HasValue)
            countQuery = countQuery.Where(b => b.MedicineId == query.MedicineId.Value);
        if (query.CategoryId.HasValue)
            countQuery = countQuery.Where(b => b.Medicine.CategoryId == query.CategoryId.Value);
        if (!string.IsNullOrEmpty(query.Search))
        {
            var searchTerm = query.Search.ToLower();
            countQuery = countQuery.Where(b =>
                b.Medicine.Name.ToLower().Contains(searchTerm) ||
                (b.BatchBarcode != null && b.BatchBarcode.Contains(searchTerm)) ||
                b.CompanyBatchNumber.Contains(searchTerm));
        }

        var totalCount = await countQuery.CountAsync();

        return new PagedResponse<BatchValuationDto>(fullResult.Batches, totalCount, query.Page, query.PageSize);
    }

    // ===================== ملخص التقارير - Reports Summary =====================

    /// <inheritdoc/>
    public async Task<ReportsSummaryDto> GetReportsSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var result = new ReportsSummaryDto
        {
            // Customer debts (positive balance means they owe us)
            TotalCustomerDebts = await _context.Customers
                .AsNoTracking()
                .Where(c => c.IsActive && c.Balance > 0)
                .SumAsync(c => (decimal?)c.Balance) ?? 0,

            // Supplier debts (balance = what we owe them)
            TotalSupplierDebts = await _context.Suppliers
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.Balance > 0)
                .SumAsync(s => (decimal?)s.Balance) ?? 0,

            // Inventory capital
            InventoryCapital = await _context.MedicineBatches
                .AsNoTracking()
                .Where(b => !b.IsDeleted && b.RemainingQuantity > 0 && b.ExpiryDate > today)
                .SumAsync(b => (decimal?)b.RemainingQuantity * b.UnitPurchasePrice) ?? 0,

            // Current month net profit
            CurrentMonthNetProfit = await CalculateMonthNetProfitAsync(startOfMonth, today),

            // Expiring soon batches
            ExpiringSoonBatches = await _context.MedicineBatches
                .AsNoTracking()
                .CountAsync(b => !b.IsDeleted
                    && b.RemainingQuantity > 0
                    && b.ExpiryDate >= today
                    && b.ExpiryDate <= today.AddDays(30))
        };

        return result;
    }

    private async Task<decimal> CalculateMonthNetProfitAsync(DateTime from, DateTime to)
    {
        var sales = await _context.SaleInvoices
            .AsNoTracking()
            .Where(i => i.Status == DocumentStatus.Approved && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .SumAsync(i => (decimal?)i.TotalProfit) ?? 0;

        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        return sales - expenses;
    }

    // ===================== تقرير المبيعات اليومية - Daily Sales Report =====================

    /// <inheritdoc/>
    public async Task<DailySalesReportDto> GetDailySalesReportAsync(DateTime date)
    {
        _logger.LogInformation("Generating daily sales report for {Date}", date.ToString("yyyy-MM-dd"));

        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        var result = new DailySalesReportDto { Date = date };

        // Get all approved invoices for the day
        var invoices = await _context.SaleInvoices
            .AsNoTracking()
            .Where(i => i.Status == DocumentStatus.Approved
                && i.InvoiceDate >= startOfDay
                && i.InvoiceDate <= endOfDay)
            .Select(i => new
            {
                i.TotalAmount,
                i.TotalCost,
                i.PaymentMethod,
                Hour = i.InvoiceDate.Hour,
                ItemCount = i.SaleInvoiceDetails.Sum(d => d.Quantity)
            })
            .ToListAsync();

        result.TotalSales = invoices.Sum(i => i.TotalAmount);
        result.TotalCost = invoices.Sum(i => i.TotalCost);
        result.InvoiceCount = invoices.Count;
        result.ItemsSold = invoices.Sum(i => i.ItemCount);
        result.CashSales = invoices.Where(i => i.PaymentMethod == PaymentType.Cash).Sum(i => i.TotalAmount);
        result.CreditSales = invoices.Where(i => i.PaymentMethod == PaymentType.Credit).Sum(i => i.TotalAmount);

        // Sales by hour
        result.SalesByHour = Enumerable.Range(0, 24)
            .Select(h => new HourlySalesDto
            {
                Hour = h,
                Amount = invoices.Where(i => i.Hour == h).Sum(i => i.TotalAmount),
                InvoiceCount = invoices.Count(i => i.Hour == h)
            })
            .Where(h => h.Amount > 0 || h.InvoiceCount > 0)
            .ToList();

        // Top 5 selling items
        result.TopSellingItems = await _context.SaleInvoiceDetails
            .AsNoTracking()
            .Where(d => d.SaleInvoice.Status == DocumentStatus.Approved
                && d.SaleInvoice.InvoiceDate >= startOfDay
                && d.SaleInvoice.InvoiceDate <= endOfDay)
            .GroupBy(d => new { d.MedicineId, d.Medicine.Name })
            .Select(g => new TopSellingItemDto
            {
                MedicineId = g.Key.MedicineId,
                MedicineName = g.Key.Name,
                QuantitySold = g.Sum(d => d.Quantity),
                Revenue = g.Sum(d => d.TotalLineAmount)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();

        return result;
    }

    // ===================== تقرير الأدوية الأكثر مبيعاً - Best Selling Report =====================

    /// <inheritdoc/>
    public async Task<BestSellingMedicinesReportDto> GetBestSellingMedicinesAsync(DateTime fromDate, DateTime toDate, int top = 10)
    {
        _logger.LogInformation("Generating best selling medicines report from {From} to {To}, top {Top}", fromDate, toDate, top);

        var result = new BestSellingMedicinesReportDto
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        var medicines = await _context.SaleInvoiceDetails
            .AsNoTracking()
            .Where(d => d.SaleInvoice.Status == DocumentStatus.Approved
                && d.SaleInvoice.InvoiceDate >= fromDate
                && d.SaleInvoice.InvoiceDate <= toDate)
            .GroupBy(d => new
            {
                d.MedicineId,
                d.Medicine.Name,
                d.Medicine.ScientificName,
                CategoryName = d.Medicine.Category.Name
            })
            .Select(g => new BestSellingMedicineDto
            {
                MedicineId = g.Key.MedicineId,
                MedicineName = g.Key.Name,
                ScientificName = g.Key.ScientificName,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(d => d.Quantity),
                TotalRevenue = g.Sum(d => d.TotalLineAmount),
                TotalProfit = g.Sum(d => d.TotalLineAmount - (d.Quantity * d.UnitCost)),
                InvoiceCount = g.Select(d => d.SaleInvoiceId).Distinct().Count(),
                LastSaleDate = g.Max(d => d.SaleInvoice.InvoiceDate)
            })
            .OrderByDescending(m => m.QuantitySold)
            .Take(top)
            .ToListAsync();

        // Add ranking
        int rank = 1;
        foreach (var medicine in medicines)
        {
            medicine.Rank = rank++;
        }

        result.Medicines = medicines;
        result.TotalMedicinesSold = medicines.Sum(m => m.QuantitySold);
        result.TotalRevenue = medicines.Sum(m => m.TotalRevenue);

        return result;
    }

    // ===================== تقرير ديون العملاء - Customer Debts Report =====================

    /// <inheritdoc/>
    public async Task<CustomerDebtsReportDto> GetCustomerDebtsReportAsync()
    {
        _logger.LogInformation("Generating customer debts report");

        var result = new CustomerDebtsReportDto();

        var customers = await _context.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CustomerDebtDto
            {
                CustomerId = c.Id,
                CustomerName = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                Balance = c.Balance,
                TotalPurchases = c.SaleInvoices
                    .Where(i => i.Status == DocumentStatus.Approved)
                    .Sum(i => (decimal?)i.TotalAmount) ?? 0,
                TotalPayments = c.Receipts
                    .Where(r => !r.IsCancelled)
                    .Sum(r => (decimal?)r.Amount) ?? 0,
                OutstandingInvoices = c.SaleInvoices
                    .Count(i => i.Status == DocumentStatus.Approved && i.PaymentMethod == PaymentType.Credit),
                LastInvoiceDate = c.SaleInvoices
                    .Where(i => i.Status == DocumentStatus.Approved)
                    .Max(i => (DateTime?)i.InvoiceDate),
                LastPaymentDate = c.Receipts
                    .Where(r => !r.IsCancelled)
                    .Max(r => (DateTime?)r.ReceiptDate)
            })
            .Where(c => c.Balance != 0)
            .OrderByDescending(c => Math.Abs(c.Balance))
            .ToListAsync();

        // Calculate days overdue
        var today = DateTime.UtcNow.Date;
        foreach (var customer in customers)
        {
            if (customer.Balance > 0 && customer.LastInvoiceDate.HasValue)
            {
                customer.DaysOverdue = (int)(today - customer.LastInvoiceDate.Value.Date).TotalDays;
            }
        }

        result.Customers = customers;
        result.TotalReceivable = customers.Where(c => c.Balance > 0).Sum(c => c.Balance);
        result.TotalPayable = customers.Where(c => c.Balance < 0).Sum(c => Math.Abs(c.Balance));
        result.DebtorCount = customers.Count(c => c.Balance > 0);
        result.CreditorCount = customers.Count(c => c.Balance < 0);

        return result;
    }

    public async Task<EmployeePerformanceReportDto> GetEmployeePerformanceReportAsync(EmployeePerformanceReportQueryDto query)
    {
        var fromDate = query.FromDate?.Date;
        var toDate = query.ToDate?.Date.AddDays(1).AddTicks(-1);
        var operationType = string.IsNullOrWhiteSpace(query.OperationType) ? "All" : query.OperationType.Trim();

        var includeSales = operationType.Equals("All", StringComparison.OrdinalIgnoreCase)
            || operationType.Equals("Sales", StringComparison.OrdinalIgnoreCase);
        var includeReturns = operationType.Equals("All", StringComparison.OrdinalIgnoreCase)
            || operationType.Equals("Returns", StringComparison.OrdinalIgnoreCase);

        if (!includeSales && !includeReturns)
            throw new ArgumentException("OperationType must be All, Sales, or Returns.");

        var result = new EmployeePerformanceReportDto
        {
            FromDate = fromDate,
            ToDate = query.ToDate?.Date,
            EmployeeId = query.EmployeeId,
            RoleId = query.RoleId,
            OperationType = operationType
        };

        var employeesQuery = _context.Users.AsNoTracking().Where(u => !u.IsDeleted);

        if (query.EmployeeId.HasValue)
            employeesQuery = employeesQuery.Where(u => u.Id == query.EmployeeId.Value);

        if (query.RoleId.HasValue)
            employeesQuery = employeesQuery.Where(u => u.RoleId == query.RoleId.Value);

        var employees = await employeesQuery
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                u.RoleId,
                RoleName = u.Role.Name
            })
            .ToListAsync();

        var employeeIds = employees.Select(e => e.Id).ToList();
        if (employeeIds.Count == 0)
            return result;

        var salesQuery = _context.SaleInvoices
            .AsNoTracking()
            .Where(i => i.Status == DocumentStatus.Approved
                && employeeIds.Contains(i.CreatedBy)
                && (!fromDate.HasValue || i.InvoiceDate >= fromDate.Value)
                && (!toDate.HasValue || i.InvoiceDate <= toDate.Value));

        var returnsQuery = _context.SalesReturns
            .AsNoTracking()
            .Where(r => r.Status == DocumentStatus.Approved
                && employeeIds.Contains(r.CreatedBy)
                && (!fromDate.HasValue || r.ReturnDate >= fromDate.Value)
                && (!toDate.HasValue || r.ReturnDate <= toDate.Value));

        var salesSummary = includeSales
            ? await salesQuery
                .GroupBy(i => i.CreatedBy)
                .Select(g => new EmployeePerformanceSummaryDto
                {
                    EmployeeId = g.Key,
                    TotalSales = g.Sum(i => i.TotalAmount),
                    SalesInvoiceCount = g.Count(),
                    ItemsSold = g.Sum(i => i.SaleInvoiceDetails.Sum(d => d.Quantity))
                })
                .ToDictionaryAsync(x => x.EmployeeId)
            : new Dictionary<int, EmployeePerformanceSummaryDto>();

        var returnsSummary = includeReturns
            ? await returnsQuery
                .GroupBy(r => r.CreatedBy)
                .Select(g => new EmployeePerformanceSummaryDto
                {
                    EmployeeId = g.Key,
                    TotalReturns = g.Sum(r => r.TotalAmount),
                    SalesReturnCount = g.Count(),
                    ItemsReturned = g.Sum(r => r.SalesReturnDetails.Sum(d => d.Quantity))
                })
                .ToDictionaryAsync(x => x.EmployeeId)
            : new Dictionary<int, EmployeePerformanceSummaryDto>();

        result.Employees = employees
            .Select(employee =>
            {
                salesSummary.TryGetValue(employee.Id, out var sale);
                returnsSummary.TryGetValue(employee.Id, out var ret);

                return new EmployeePerformanceSummaryDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    Username = employee.Username,
                    RoleId = employee.RoleId,
                    RoleName = employee.RoleName,
                    TotalSales = sale?.TotalSales ?? 0,
                    SalesInvoiceCount = sale?.SalesInvoiceCount ?? 0,
                    ItemsSold = sale?.ItemsSold ?? 0,
                    TotalReturns = ret?.TotalReturns ?? 0,
                    SalesReturnCount = ret?.SalesReturnCount ?? 0,
                    ItemsReturned = ret?.ItemsReturned ?? 0
                };
            })
            .Where(e => e.SalesInvoiceCount > 0 || e.SalesReturnCount > 0)
            .OrderByDescending(e => e.NetSales)
            .ToList();

        result.TotalSales = result.Employees.Sum(e => e.TotalSales);
        result.SalesInvoiceCount = result.Employees.Sum(e => e.SalesInvoiceCount);
        result.ItemsSold = result.Employees.Sum(e => e.ItemsSold);
        result.TotalReturns = result.Employees.Sum(e => e.TotalReturns);
        result.SalesReturnCount = result.Employees.Sum(e => e.SalesReturnCount);
        result.ItemsReturned = result.Employees.Sum(e => e.ItemsReturned);

        if (includeSales)
        {
            result.SaleInvoices = await salesQuery
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.CreatedAt)
                .Select(i => new EmployeeSalesInvoiceDto
                {
                    InvoiceId = i.Id,
                    InvoiceNumber = i.SaleInvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    CreatedAt = i.CreatedAt,
                    EmployeeId = i.CreatedBy,
                    EmployeeName = i.Creator != null ? i.Creator.FullName : "Unknown",
                    RoleName = i.Creator != null ? i.Creator.Role.Name : string.Empty,
                    CustomerName = i.CustomerName ?? (i.Customer != null ? i.Customer.Name : null),
                    PaymentMethod = i.PaymentMethod.ToString(),
                    TotalAmount = i.TotalAmount,
                    ItemsCount = i.SaleInvoiceDetails.Sum(d => d.Quantity),
                    Items = i.SaleInvoiceDetails
                        .Select(d => new EmployeeOperationItemDto
                        {
                            MedicineId = d.MedicineId,
                            MedicineName = d.Medicine.Name,
                            BatchId = d.BatchId,
                            BatchNumber = d.Batch.CompanyBatchNumber,
                            Quantity = d.Quantity,
                            UnitPrice = d.SalePrice,
                            TotalAmount = d.TotalLineAmount
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        if (includeReturns)
        {
            result.SalesReturns = await returnsQuery
                .OrderByDescending(r => r.ReturnDate)
                .ThenByDescending(r => r.CreatedAt)
                .Select(r => new EmployeeSalesReturnDto
                {
                    ReturnId = r.Id,
                    SaleInvoiceId = r.SaleInvoiceId,
                    SaleInvoiceNumber = r.SaleInvoice.SaleInvoiceNumber,
                    ReturnDate = r.ReturnDate,
                    CreatedAt = r.CreatedAt,
                    EmployeeId = r.CreatedBy,
                    EmployeeName = r.Creator != null ? r.Creator.FullName : "Unknown",
                    RoleName = r.Creator != null ? r.Creator.Role.Name : string.Empty,
                    CustomerName = r.Customer != null ? r.Customer.Name : r.SaleInvoice.CustomerName,
                    Reason = r.Reason,
                    TotalAmount = r.TotalAmount,
                    ItemsCount = r.SalesReturnDetails.Sum(d => d.Quantity),
                    Items = r.SalesReturnDetails
                        .Select(d => new EmployeeOperationItemDto
                        {
                            MedicineId = d.MedicineId,
                            MedicineName = d.Medicine.Name,
                            BatchId = d.BatchId,
                            BatchNumber = d.Batch.CompanyBatchNumber,
                            Quantity = d.Quantity,
                            UnitPrice = d.SalePrice,
                            TotalAmount = d.TotalLineAmount
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        return result;
    }

    // ===================== تقرير ديون الموردين - Supplier Debts Report =====================

    /// <inheritdoc/>
    public async Task<SupplierDebtsReportDto> GetSupplierDebtsReportAsync()
    {
        _logger.LogInformation("Generating supplier debts report");

        var result = new SupplierDebtsReportDto();

        // 1. Get payment stats separately (since no navigation property on Supplier)
        var paymentStats = await _context.SupplierPayments
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                TotalPayments = g.Sum(p => p.Amount),
                LastPaymentDate = g.Max(p => p.PaymentDate)
            })
            .ToDictionaryAsync(x => x.SupplierId);

        // 2. Get supplier invoice stats
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Select(s => new SupplierDebtDto
            {
                SupplierId = s.Id,
                SupplierName = s.Name,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                ContactPerson = "", // Field removed from entity or not available
                Balance = s.Balance,
                TotalPurchases = s.PurchaseInvoices
                    .Where(i => i.Status == DocumentStatus.Approved)
                    .Sum(i => (decimal?)i.TotalAmount) ?? 0,
                // Payments will be populated from dictionary
                TotalPayments = 0,
                OutstandingInvoices = s.PurchaseInvoices
                    .Count(i => i.Status == DocumentStatus.Approved && i.PaymentMethod == PaymentType.Credit),
                LastPurchaseDate = s.PurchaseInvoices
                    .Where(i => i.Status == DocumentStatus.Approved)
                    .Max(i => (DateTime?)i.PurchaseDate),
                // Last payment date will be populated from dictionary
                LastPaymentDate = null
            })
            .Where(s => s.Balance != 0)
            .OrderByDescending(s => Math.Abs(s.Balance))
            .ToListAsync();

        // 3. Merge payment data and calculate days overdue
        var today = DateTime.UtcNow.Date;
        foreach (var supplier in suppliers)
        {
            if (paymentStats.TryGetValue(supplier.SupplierId, out var stats))
            {
                supplier.TotalPayments = stats.TotalPayments;
                supplier.LastPaymentDate = stats.LastPaymentDate;
            }

            if (supplier.Balance > 0 && supplier.LastPurchaseDate.HasValue)
            {
                supplier.DaysOverdue = (int)(today - supplier.LastPurchaseDate.Value.Date).TotalDays;
            }
        }

        result.Suppliers = suppliers;
        result.TotalPayable = suppliers.Where(s => s.Balance > 0).Sum(s => s.Balance);
        result.TotalReceivable = suppliers.Where(s => s.Balance < 0).Sum(s => Math.Abs(s.Balance));
        result.CreditorCount = suppliers.Count(s => s.Balance > 0);
        result.DebtorCount = suppliers.Count(s => s.Balance < 0);

        return result;
    }
}


