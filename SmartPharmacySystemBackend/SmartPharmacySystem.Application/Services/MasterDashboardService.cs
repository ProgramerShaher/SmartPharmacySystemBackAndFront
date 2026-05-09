using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Dashboard;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class MasterDashboardService : IMasterDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MasterDashboardService> _logger;

    public MasterDashboardService(IUnitOfWork unitOfWork, ILogger<MasterDashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MasterDashboardStatsDto> GetMasterDashboardStatsAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-30);

            var sales = ((await _unitOfWork.SaleInvoices.GetAllAsync()) ?? Enumerable.Empty<SaleInvoice>()).Where(x => !x.IsDeleted).ToList();
            var purchases = ((await _unitOfWork.PurchaseInvoices.GetAllAsync()) ?? Enumerable.Empty<PurchaseInvoice>()).Where(x => !x.IsDeleted).ToList();
            var salesReturns = ((await _unitOfWork.SalesReturns.GetAllAsync()) ?? Enumerable.Empty<SalesReturn>()).Where(x => !x.IsDeleted).ToList();
            var purchaseReturns = ((await _unitOfWork.PurchaseReturns.GetAllAsync()) ?? Enumerable.Empty<PurchaseReturn>()).Where(x => !x.IsDeleted).ToList();
            var medicines = ((await _unitOfWork.Medicines.GetAllAsync()) ?? Enumerable.Empty<Medicine>()).Where(x => !x.IsDeleted).ToList();
            var batches = ((await _unitOfWork.MedicineBatches.GetAllAsync()) ?? Enumerable.Empty<MedicineBatch>()).Where(x => !x.IsDeleted).ToList();
            var customers = ((await _unitOfWork.Customers.GetAllAsync()) ?? Enumerable.Empty<Customer>()).ToList();
            var suppliers = ((await _unitOfWork.Suppliers.GetAllAsync()) ?? Enumerable.Empty<Supplier>()).Where(x => !x.IsDeleted).ToList();
            var users = ((await _unitOfWork.Users.GetAllAsync()) ?? Enumerable.Empty<User>()).Where(x => !x.IsDeleted).ToList();
            var expenses = ((await _unitOfWork.Expenses.GetAllAsync()) ?? Enumerable.Empty<Expense>()).Where(x => !x.IsDeleted).ToList();
            var alerts = ((await _unitOfWork.Alerts.GetAllAsync()) ?? Enumerable.Empty<Alert>()).Where(x => !x.IsDeleted).ToList();
            var supplierPayments = ((await _unitOfWork.SupplierPayments.GetAllAsync()) ?? Enumerable.Empty<SupplierPayment>()).Where(x => !x.IsDeleted).ToList();
            var customerReceiptsPage = await _unitOfWork.CustomerReceipts.GetPagedAsync(null, 1, 100000, null, null);
            var customerReceipts = customerReceiptsPage.Items.Where(x => !x.IsCancelled).ToList();
            var movements = ((await _unitOfWork.InventoryMovements.GetAllAsync()) ?? Enumerable.Empty<InventoryMovement>()).ToList();
            var financialTransactions = (await GetFinancialTransactionsSafeAsync(thirtyDaysAgo, today.AddDays(1))).ToList();
            var mainAccount = await GetMainAccountSafeAsync();

            var inventory = BuildInventoryIntelligence(batches, medicines, purchases, suppliers, alerts);

            var result = new MasterDashboardStatsDto
            {
                SystemOverview = BuildSystemOverview(
                    today,
                    sales,
                    purchases,
                    salesReturns,
                    purchaseReturns,
                    medicines,
                    customers,
                    suppliers,
                    users,
                    alerts,
                    inventory.CriticalStockItems.Count),
                FinancialIntelligence = BuildFinancialIntelligence(
                    today,
                    thirtyDaysAgo,
                    sales,
                    purchases,
                    salesReturns,
                    purchaseReturns,
                    expenses,
                    customers,
                    suppliers,
                    financialTransactions,
                    mainAccount),
                InventoryIntelligence = inventory,
                OperationalPulse = BuildOperationalPulse(
                    today,
                    sales,
                    purchases,
                    salesReturns,
                    purchaseReturns,
                    supplierPayments,
                    customerReceipts,
                    expenses,
                    movements,
                    users)
            };

            stopwatch.Stop();
            _logger.LogInformation("[MasterDashboard] Built real dashboard payload in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[MasterDashboard] Error getting stats after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static SystemOverviewDto BuildSystemOverview(
        DateTime today,
        List<SaleInvoice> sales,
        List<PurchaseInvoice> purchases,
        List<SalesReturn> salesReturns,
        List<PurchaseReturn> purchaseReturns,
        List<Medicine> medicines,
        List<Customer> customers,
        List<Supplier> suppliers,
        List<User> users,
        List<Alert> alerts,
        int criticalStockCount)
    {
        return new SystemOverviewDto
        {
            SalesInvoicesCount = sales.Count,
            PurchaseInvoicesCount = purchases.Count,
            SalesReturnsCount = salesReturns.Count,
            PurchaseReturnsCount = purchaseReturns.Count,
            MedicinesCount = medicines.Count,
            CustomersCount = customers.Count,
            SuppliersCount = suppliers.Count,
            UsersCount = users.Count,
            ActiveAlertsCount = alerts.Count(x => !x.IsRead),
            CriticalStockCount = criticalStockCount,
            TodayDocumentsCount =
                sales.Count(x => IsSameDay(x.InvoiceDate, today)) +
                purchases.Count(x => IsSameDay(x.PurchaseDate, today)) +
                salesReturns.Count(x => IsSameDay(x.ReturnDate, today)) +
                purchaseReturns.Count(x => IsSameDay(x.ReturnDate, today))
        };
    }

    private FinancialIntelligenceDto BuildFinancialIntelligence(
        DateTime today,
        DateTime thirtyDaysAgo,
        List<SaleInvoice> sales,
        List<PurchaseInvoice> purchases,
        List<SalesReturn> salesReturns,
        List<PurchaseReturn> purchaseReturns,
        List<Expense> expenses,
        List<Customer> customers,
        List<Supplier> suppliers,
        List<FinancialTransaction> transactions,
        PharmacyAccount? mainAccount)
    {
        var approvedSales = sales.Where(x => x.Status == DocumentStatus.Approved).ToList();
        var approvedPurchases = purchases.Where(x => x.Status == DocumentStatus.Approved).ToList();
        var approvedSalesReturns = salesReturns.Where(x => x.Status == DocumentStatus.Approved).ToList();
        var approvedPurchaseReturns = purchaseReturns.Where(x => x.Status == DocumentStatus.Approved).ToList();

        var todaySales = approvedSales.Where(x => IsSameDay(x.InvoiceDate, today)).ToList();
        var todayReturns = approvedSalesReturns.Where(x => IsSameDay(x.ReturnDate, today)).ToList();
        var todayExpenses = expenses.Where(x => IsSameDay(x.ExpenseDate, today)).ToList();

        var last30Sales = approvedSales.Where(x => IsInRange(x.InvoiceDate, thirtyDaysAgo, today.AddDays(1))).ToList();
        var last30Purchases = approvedPurchases.Where(x => IsInRange(x.PurchaseDate, thirtyDaysAgo, today.AddDays(1))).ToList();
        var last30SalesReturns = approvedSalesReturns.Where(x => IsInRange(x.ReturnDate, thirtyDaysAgo, today.AddDays(1))).ToList();
        var last30PurchaseReturns = approvedPurchaseReturns.Where(x => IsInRange(x.ReturnDate, thirtyDaysAgo, today.AddDays(1))).ToList();
        var last30Expenses = expenses.Where(x => IsInRange(x.ExpenseDate, thirtyDaysAgo, today.AddDays(1))).ToList();

        var todaySalesAmount = todaySales.Sum(x => x.TotalAmount);
        var todayCOGS = todaySales.Sum(x => x.TotalCost);
        var todayReturnsAmount = todayReturns.Sum(x => x.TotalAmount);
        var todayExpensesAmount = todayExpenses.Sum(x => x.Amount);

        var last30ReturnsAmount = last30SalesReturns.Sum(x => x.TotalAmount) + last30PurchaseReturns.Sum(x => x.TotalAmount);
        var last30ExpensesAmount = last30Expenses.Sum(x => x.Amount);
        var last30NetProfit = last30Sales.Sum(x => x.TotalAmount) -
                              last30SalesReturns.Sum(x => x.TotalAmount) -
                              last30Sales.Sum(x => x.TotalCost) -
                              last30ExpensesAmount;

        var receivables = customers.Where(x => x.Balance > 0).Sum(x => x.Balance);
        var payables = suppliers.Where(x => x.Balance > 0).Sum(x => x.Balance);

        return new FinancialIntelligenceDto
        {
            TodayApprovedSales = todaySalesAmount,
            TodayReturns = todayReturnsAmount,
            TodayCOGS = todayCOGS,
            TodayExpenses = todayExpensesAmount,
            TodayNetProfit = todaySalesAmount - todayReturnsAmount - todayCOGS - todayExpensesAmount,
            TotalSalesLast30Days = last30Sales.Sum(x => x.TotalAmount),
            TotalPurchasesLast30Days = last30Purchases.Sum(x => x.TotalAmount),
            TotalReturnsLast30Days = last30ReturnsAmount,
            TotalExpensesLast30Days = last30ExpensesAmount,
            NetProfitLast30Days = last30NetProfit,
            CashBalance = mainAccount?.Balance ?? 0,
            BankBalance = 0,
            TotalLiquidity = mainAccount?.Balance ?? 0,
            CustomerReceivables = receivables,
            SupplierPayables = payables,
            NetDebt = receivables - payables,
            CashFlowInLast30Days = BuildCashFlow(transactions, FinancialTransactionType.Income, thirtyDaysAgo, today),
            CashFlowOutLast30Days = BuildCashFlow(transactions, FinancialTransactionType.Expense, thirtyDaysAgo, today),
            BranchRevenues = new List<BranchRevenueDto>
            {
                new() { BranchName = "الصيدلية الرئيسية", Revenue = last30Sales.Sum(x => x.TotalAmount) }
            }
        };
    }

    private static InventoryIntelligenceDto BuildInventoryIntelligence(
        List<MedicineBatch> batches,
        List<Medicine> medicines,
        List<PurchaseInvoice> purchases,
        List<Supplier> suppliers,
        List<Alert> alerts)
    {
        var now = DateTime.UtcNow;
        var inventoryBySupplier = batches
            .Where(x => x.PurchaseInvoiceId.HasValue && x.RemainingQuantity > 0)
            .Join(purchases, b => b.PurchaseInvoiceId!.Value, p => p.Id, (b, p) => new { Batch = b, p.SupplierId })
            .Join(suppliers, x => x.SupplierId, s => s.Id, (x, s) => new { x.Batch, Supplier = s })
            .GroupBy(x => new { x.Supplier.Id, x.Supplier.Name })
            .Select(g => new SupplierInventoryDto
            {
                SupplierId = g.Key.Id,
                SupplierName = g.Key.Name,
                InventoryValue = g.Sum(x => x.Batch.RemainingQuantity * x.Batch.UnitPurchasePrice),
                ItemCount = g.Select(x => x.Batch.MedicineId).Distinct().Count()
            })
            .OrderByDescending(x => x.InventoryValue)
            .Take(8)
            .ToList();

        if (!inventoryBySupplier.Any())
        {
            inventoryBySupplier = batches
                .Where(x => x.RemainingQuantity > 0)
                .Join(medicines, b => b.MedicineId, m => m.Id, (b, m) => new { Batch = b, Medicine = m })
                .GroupBy(x => x.Medicine.CategoryId)
                .Select(g => new SupplierInventoryDto
                {
                    SupplierId = g.Key ?? 0,
                    SupplierName = g.Key.HasValue ? $"تصنيف {g.Key}" : "غير مصنف",
                    InventoryValue = g.Sum(x => x.Batch.RemainingQuantity * x.Batch.UnitPurchasePrice),
                    ItemCount = g.Select(x => x.Batch.MedicineId).Distinct().Count()
                })
                .OrderByDescending(x => x.InventoryValue)
                .Take(8)
                .ToList();
        }

        return new InventoryIntelligenceDto
        {
            TotalInventoryValue = batches.Sum(x => x.RemainingQuantity * x.UnitPurchasePrice),
            TotalMedicines = medicines.Count,
            TotalBatches = batches.Count,
            ActiveBatches = batches.Count(x => x.Status == "Active" && x.RemainingQuantity > 0),
            ExpiredBatches = batches.Count(x => x.ExpiryDate.Date < now.Date),
            NearExpiryBatches = batches.Count(x => x.ExpiryDate.Date >= now.Date && x.ExpiryDate.Date <= now.AddMonths(3).Date),
            TotalStockQuantity = batches.Sum(x => x.RemainingQuantity),
            ActiveAlerts = alerts.Count(x => !x.IsRead),
            InventoryBySupplier = inventoryBySupplier,
            ExpiryRadar = CalculateExpiryRadar(batches),
            CriticalStockItems = GetCriticalStockItems(batches, medicines)
        };
    }

    private OperationalPulseDto BuildOperationalPulse(
        DateTime today,
        List<SaleInvoice> sales,
        List<PurchaseInvoice> purchases,
        List<SalesReturn> salesReturns,
        List<PurchaseReturn> purchaseReturns,
        List<SupplierPayment> supplierPayments,
        List<CustomerReceipt> customerReceipts,
        List<Expense> expenses,
        List<InventoryMovement> movements,
        List<User> users)
    {
        var userNames = users.ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.FullName) ? x.Username : x.FullName);
        var approvedSalesToday = sales
            .Where(x => x.Status == DocumentStatus.Approved && IsSameDay(x.InvoiceDate, today))
            .ToList();

        return new OperationalPulseDto
        {
            ActivityStream = BuildActivityStream(
                sales,
                purchases,
                salesReturns,
                purchaseReturns,
                supplierPayments,
                customerReceipts,
                expenses,
                movements,
                userNames),
            CashierPerformance = approvedSalesToday
                .GroupBy(x => x.CreatedBy)
                .Select(g => new CashierPerformanceDto
                {
                    UserId = g.Key,
                    Username = GetUserName(userNames, g.Key),
                    TotalSales = g.Sum(x => x.TotalAmount),
                    InvoiceCount = g.Count(),
                    AverageInvoiceValue = g.Count() == 0 ? 0 : g.Average(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(10)
                .ToList(),
            HourlyHeatMap = BuildHourlyHeatMap(approvedSalesToday)
        };
    }

    private static List<ActivityStreamItemDto> BuildActivityStream(
        List<SaleInvoice> sales,
        List<PurchaseInvoice> purchases,
        List<SalesReturn> salesReturns,
        List<PurchaseReturn> purchaseReturns,
        List<SupplierPayment> supplierPayments,
        List<CustomerReceipt> customerReceipts,
        List<Expense> expenses,
        List<InventoryMovement> movements,
        Dictionary<int, string> userNames)
    {
        var activities = new List<ActivityStreamItemDto>();

        activities.AddRange(sales.Where(x => x.Status == DocumentStatus.Approved).Select(x => new ActivityStreamItemDto
        {
            OperationType = "SaleInvoice",
            DocumentNumber = x.SaleInvoiceNumber,
            Amount = x.TotalAmount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ApprovedAt ?? x.CreatedAt,
            ReferenceId = x.Id,
            Description = $"فاتورة مبيعات #{x.SaleInvoiceNumber}",
            SourceRoute = $"/sales/{x.Id}",
            EntityName = x.CustomerName ?? "عميل نقدي"
        }));

        activities.AddRange(purchases.Where(x => x.Status == DocumentStatus.Approved).Select(x => new ActivityStreamItemDto
        {
            OperationType = "PurchaseInvoice",
            DocumentNumber = x.PurchaseInvoiceNumber,
            Amount = x.TotalAmount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ApprovedAt ?? x.CreatedAt,
            ReferenceId = x.Id,
            Description = $"فاتورة مشتريات #{x.PurchaseInvoiceNumber}",
            SourceRoute = $"/purchases/{x.Id}",
            EntityName = x.SupplierInvoiceNumber ?? "مورد"
        }));

        activities.AddRange(salesReturns.Where(x => x.Status == DocumentStatus.Approved).Select(x => new ActivityStreamItemDto
        {
            OperationType = "SalesReturn",
            DocumentNumber = x.Id.ToString(),
            Amount = x.TotalAmount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ApprovedAt ?? x.CreatedAt,
            ReferenceId = x.Id,
            Description = $"مردود مبيعات #{x.Id}",
            SourceRoute = $"/sales/returns/{x.Id}",
            EntityName = x.Reason ?? "مرتجع"
        }));

        activities.AddRange(purchaseReturns.Where(x => x.Status == DocumentStatus.Approved).Select(x => new ActivityStreamItemDto
        {
            OperationType = "PurchaseReturn",
            DocumentNumber = x.Id.ToString(),
            Amount = x.TotalAmount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ApprovedAt ?? x.CreatedAt,
            ReferenceId = x.Id,
            Description = $"مردود مشتريات #{x.Id}",
            SourceRoute = $"/purchases/returns/{x.Id}",
            EntityName = x.Reason ?? "مرتجع"
        }));

        activities.AddRange(supplierPayments.Select(x => new ActivityStreamItemDto
        {
            OperationType = "SupplierPayment",
            DocumentNumber = x.ReferenceNo ?? x.Id.ToString(),
            Amount = x.Amount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.PaymentDate,
            ReferenceId = x.Id,
            Description = $"سند صرف مورد #{x.ReferenceNo ?? x.Id.ToString()}",
            SourceRoute = "/partners/suppliers/payments",
            EntityName = "مورد"
        }));

        activities.AddRange(customerReceipts.Select(x => new ActivityStreamItemDto
        {
            OperationType = "CustomerReceipt",
            DocumentNumber = x.ReferenceNo ?? x.Id.ToString(),
            Amount = x.Amount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ReceiptDate,
            ReferenceId = x.Id,
            Description = $"سند قبض عميل #{x.ReferenceNo ?? x.Id.ToString()}",
            SourceRoute = "/customers/receipts",
            EntityName = "عميل"
        }));

        activities.AddRange(expenses.Select(x => new ActivityStreamItemDto
        {
            OperationType = "Expense",
            DocumentNumber = x.Id.ToString(),
            Amount = x.Amount,
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.ExpenseDate,
            ReferenceId = x.Id,
            Description = $"مصروف #{x.Id}",
            SourceRoute = $"/finance/expenses/edit/{x.Id}",
            EntityName = x.Notes ?? "مصروف"
        }));

        activities.AddRange(movements.Select(x => new ActivityStreamItemDto
        {
            OperationType = "InventoryMovement",
            DocumentNumber = x.ReferenceNumber,
            Amount = Math.Abs(x.Quantity),
            Username = GetUserName(userNames, x.CreatedBy),
            Timestamp = x.Date,
            ReferenceId = x.Id,
            Description = $"حركة مخزون #{x.ReferenceNumber}",
            SourceRoute = $"/inventory/movements/{x.Id}",
            EntityName = x.Notes
        }));

        return activities
            .OrderByDescending(x => x.Timestamp)
            .Take(18)
            .ToList();
    }

    private static ExpiryRadarDto CalculateExpiryRadar(List<MedicineBatch> batches)
    {
        var now = DateTime.UtcNow;
        var activeBatches = batches.Where(x => x.RemainingQuantity > 0).ToList();
        var total = activeBatches.Count;

        if (total == 0)
        {
            return new ExpiryRadarDto();
        }

        return new ExpiryRadarDto
        {
            PercentageLessThan3Months = Math.Round(activeBatches.Count(x => x.ExpiryDate <= now.AddMonths(3)) * 100m / total, 2),
            Percentage3To6Months = Math.Round(activeBatches.Count(x => x.ExpiryDate > now.AddMonths(3) && x.ExpiryDate <= now.AddMonths(6)) * 100m / total, 2),
            Percentage6To12Months = Math.Round(activeBatches.Count(x => x.ExpiryDate > now.AddMonths(6) && x.ExpiryDate <= now.AddMonths(12)) * 100m / total, 2),
            PercentageMoreThan12Months = Math.Round(activeBatches.Count(x => x.ExpiryDate > now.AddMonths(12)) * 100m / total, 2)
        };
    }

    private static List<CriticalStockItemDto> GetCriticalStockItems(List<MedicineBatch> batches, List<Medicine> medicines)
    {
        var stocks = batches
            .Where(x => x.RemainingQuantity > 0)
            .GroupBy(x => x.MedicineId)
            .Select(x => new { MedicineId = x.Key, Quantity = x.Sum(b => b.RemainingQuantity) })
            .ToList();

        return medicines
            .GroupJoin(stocks, m => m.Id, s => s.MedicineId, (medicine, stockGroup) => new { medicine, stock = stockGroup.FirstOrDefault() })
            .Select(x => new
            {
                x.medicine,
                Quantity = x.stock?.Quantity ?? 0,
                ReorderLevel = x.medicine.ReorderLevel > 0 ? x.medicine.ReorderLevel : Math.Max(x.medicine.MinAlertQuantity, 10)
            })
            .Where(x => x.Quantity <= x.ReorderLevel)
            .OrderBy(x => x.Quantity)
            .Take(12)
            .Select(x => new CriticalStockItemDto
            {
                MedicineId = x.medicine.Id,
                MedicineName = x.medicine.Name,
                CurrentStock = x.Quantity,
                ReorderPoint = x.ReorderLevel,
                SuggestedOrderQuantity = Math.Max(x.ReorderLevel * 3 - x.Quantity, x.ReorderLevel)
            })
            .ToList();
    }

    private static List<HourlyHeatMapDto> BuildHourlyHeatMap(List<SaleInvoice> todaySales)
    {
        var map = todaySales
            .GroupBy(x => x.InvoiceDate.Hour)
            .ToDictionary(
                x => x.Key,
                x => new HourlyHeatMapDto
                {
                    Hour = x.Key,
                    TotalSales = x.Sum(i => i.TotalAmount),
                    TransactionCount = x.Count()
                });

        return Enumerable.Range(0, 24)
            .Select(hour => map.TryGetValue(hour, out var item)
                ? item
                : new HourlyHeatMapDto { Hour = hour, TotalSales = 0, TransactionCount = 0 })
            .ToList();
    }

    private static List<DailyCashFlowDto> BuildCashFlow(
        List<FinancialTransaction> transactions,
        FinancialTransactionType type,
        DateTime start,
        DateTime end)
    {
        var byDate = transactions
            .Where(x => x.Type == type && IsInRange(x.TransactionDate, start, end.AddDays(1)))
            .GroupBy(x => x.TransactionDate.Date)
            .ToDictionary(x => x.Key, x => x.Sum(t => t.Amount));

        return Enumerable.Range(0, (end - start).Days + 1)
            .Select(offset =>
            {
                var date = start.AddDays(offset).Date;
                return new DailyCashFlowDto
                {
                    Date = date,
                    Amount = byDate.TryGetValue(date, out var amount) ? amount : 0
                };
            })
            .ToList();
    }

    private async Task<IEnumerable<FinancialTransaction>> GetFinancialTransactionsSafeAsync(DateTime start, DateTime end)
    {
        try
        {
            return await _unitOfWork.Financials.GetTransactionsAsync(start, end, null, 0, 100000);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MasterDashboard] Could not load financial transactions");
            return Enumerable.Empty<FinancialTransaction>();
        }
    }

    private async Task<PharmacyAccount?> GetMainAccountSafeAsync()
    {
        try
        {
            return await _unitOfWork.Financials.GetMainAccountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MasterDashboard] Could not load main pharmacy account");
            return null;
        }
    }

    private static bool IsSameDay(DateTime date, DateTime day) => date.Date == day.Date || date.Date == DateTime.Today;

    private static bool IsInRange(DateTime date, DateTime startInclusive, DateTime endExclusive)
    {
        var value = date.Date;
        return value >= startInclusive.Date && value < endExclusive.Date;
    }

    private static string GetUserName(Dictionary<int, string> users, int userId)
    {
        return users.TryGetValue(userId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : $"مستخدم #{userId}";
    }
}
