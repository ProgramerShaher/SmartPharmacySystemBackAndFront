using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.DTOs.Alerts;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Services
{
    public class AlertService(
        IAlertRepository alertRepository,
        IMedicineBatchRepository batchRepository,
        ILogger<AlertService> logger,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IMedicineBatchService medicineBatchService,
        INotificationService notificationService) : IAlertService
    {
        public async Task<IEnumerable<AlertDto>> GetAllAsync()
        {
            var alerts = await alertRepository.GetAllAsync();
            return mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        public async Task<IEnumerable<AlertDto>> SearchAsync(AlertQueryDto query)
        {
            IEnumerable<Alert> alerts;

            if (query.Status.HasValue)
            {
                var isRead = query.Status == AlertStatus.Read;
                alerts = await alertRepository.GetByReadStatusAsync(isRead);
            }
            else if (query.IsRead.HasValue)
            {
                alerts = await alertRepository.GetByReadStatusAsync(query.IsRead.Value);
            }
            else
            {
                alerts = await alertRepository.GetAllAsync();
            }
            
            // Filtering logic
            var filtered = alerts.AsQueryable();

            if (query.Severity.HasValue)
                filtered = filtered.Where(a => a.Severity == query.Severity.Value);

            if (query.BatchId.HasValue)
                filtered = filtered.Where(a => a.BatchId == query.BatchId.Value);

            return mapper.Map<IEnumerable<AlertDto>>(filtered.ToList());
        }

        public async Task<AlertDto> GetByIdAsync(int id)
        {
            var alert = await alertRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Alert {id} not found");
            return mapper.Map<AlertDto>(alert);
        }

        public async Task<AlertDto> CreateAsync(CreateAlertDto dto)
        {
            var batch = await batchRepository.GetByIdAsync(dto.BatchId)
                ?? throw new KeyNotFoundException($"Batch {dto.BatchId} not found");

            var alert = new Alert(dto.BatchId, dto.AlertType, dto.Severity, dto.Message, batch.ExpiryDate);
            await alertRepository.AddAsync(alert);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<AlertDto>(alert);
        }

        public async Task<AlertDto> UpdateAsync(int id, UpdateAlertDto dto)
        {
            var existing = await alertRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Alert {id} not found");

            existing.Severity = dto.Severity;
            existing.Message = dto.Message;
            existing.IsRead = dto.IsRead;

            await alertRepository.UpdateAsync(existing);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<AlertDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await alertRepository.DeleteAsync(id);
            await unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AlertDto>> GetByBatchIdAsync(int batchId)
        {
            var alerts = await alertRepository.GetByBatchIdAsync(batchId);
            return mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        public async Task<IEnumerable<AlertDto>> GetByStatusAsync(AlertStatus status)
        {
            var alerts = await alertRepository.GetByReadStatusAsync(status == AlertStatus.Read);
            return mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var alert = await alertRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Alert {id} not found");
            alert.IsRead = true;
            await alertRepository.UpdateAsync(alert);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task GenerateExpiryAlertsAsync()
        {
            var batches = await batchRepository.GetExpiringBatchesAsync();
            var today = DateTime.UtcNow.Date;
            bool added = false;

            foreach (var batch in batches.Where(b => !b.IsDeleted))
            {
                var daysLeft = (batch.ExpiryDate.Date - today).Days;
                
                // If strictly expired, process financial loss (auto-scrap)
                if (daysLeft <= 0 && batch.RemainingQuantity > 0 && batch.Status != "Scrapped")
                {
                    try {
                        await medicineBatchService.ProcessFinancialLossAsync(batch.Id);
                        added = true;
                        continue; // No need for expiry alert if scrapped
                    } catch (Exception ex) {
                        logger.LogError(ex, "Failed to auto-scrap expired batch {BatchId}", batch.Id);
                    }
                }

                if (daysLeft <= 30)
                {
                    // Check if unread alert already exists
                    var existing = await alertRepository.GetByBatchIdAsync(batch.Id);
                    if (existing.Any(a => !a.IsRead && (int)a.AlertType >= (int)AlertType.ExpiryTwoWeeks)) continue;

                    var severity = daysLeft <= 0 ? AlertSeverity.Critical : AlertSeverity.Warning;
                    var type = daysLeft <= 0 ? AlertType.Expired : (daysLeft <= 7 ? AlertType.ExpiryOneWeek : AlertType.ExpiryTwoWeeks);
                    
                    var alert = new Alert(batch.Id, type, severity, $"الصنف {batch.Medicine?.Name} تشغيلة {batch.CompanyBatchNumber} سينتهي خلال {daysLeft} يوم.", batch.ExpiryDate);
                    await alertRepository.AddAsync(alert);
                    added = true;
                }
            }
            if (added)
            {
                await unitOfWork.SaveChangesAsync();
                await notificationService.SendNotificationAsync("تنبيه صلاحية", "تم تحديث تنبيهات تاريخ الانتهاء", "warn");
            }
        }

        public async Task GenerateLowStockAlertsAsync()
        {
            var medicines = await unitOfWork.Medicines.GetAllAsync();
            bool added = false;
            foreach (var med in medicines.Where(m => !m.IsDeleted))
            {
                var batches = await unitOfWork.MedicineBatches.GetBatchesByMedicineIdAsync(med.Id);
                var totalQuantity = batches.Where(b => !b.IsDeleted).Sum(b => b.RemainingQuantity);
                if (totalQuantity <= med.MinAlertQuantity)
                {
                    var firstBatch = batches.FirstOrDefault();
                    if (firstBatch != null)
                    {
                        // Check if unread low stock alert exists
                        var existing = await alertRepository.GetAllAsync(); // Should ideally use a filtered repo method
                        if (existing.Any(a => !a.IsRead && a.AlertType == AlertType.LowStock && a.Batch?.MedicineId == med.Id)) continue;

                        var alert = new Alert(firstBatch.Id, AlertType.LowStock, AlertSeverity.Warning, $"نقص مخزون: {med.Name}. الرصيد الحالي: {totalQuantity}", null);
                        await alertRepository.AddAsync(alert);
                        added = true;
                    }
                }
            }
            if (added)
            {
                await unitOfWork.SaveChangesAsync();
                await notificationService.SendNotificationAsync("نقص مخزون", "تم رصد أصناف تحت حد الطلب", "error");
            }
        }

        public async Task SyncMedicineAlertsAsync(int medicineId)
        {
            logger.LogInformation("Syncing alerts for medicine {MedicineId}", medicineId);
            var batches = await unitOfWork.MedicineBatches.GetBatchesByMedicineIdAsync(medicineId);
            var today = DateTime.UtcNow.Date;
            bool added = false;

            foreach (var batch in batches.Where(b => !b.IsDeleted && b.RemainingQuantity > 0))
            {
                var daysLeft = (batch.ExpiryDate.Date - today).Days;
                if (daysLeft <= 30)
                {
                    var existing = await alertRepository.GetByBatchIdAsync(batch.Id);
                    if (existing.Any(a => !a.IsRead)) continue;

                    var severity = daysLeft <= 0 ? AlertSeverity.Critical : (daysLeft <= 14 ? AlertSeverity.Warning : AlertSeverity.Info);
                    var alert = new Alert(batch.Id, AlertType.ExpiryOneMonth, severity, $"تنبيه: {batch.Medicine?.Name} (تشغيلة {batch.CompanyBatchNumber}) سينتهي قريباً.", batch.ExpiryDate);
                    await alertRepository.AddAsync(alert);
                    added = true;
                }
            }
            
            if (added)
            {
                await unitOfWork.SaveChangesAsync();
                await notificationService.SendNotificationAsync("تحديث تنبيهات", "تم إضافة تنبيهات جديدة للصنف", "info");
            }
        }

        private bool IsBatchEligibleForAlert(MedicineBatch batch)
        {
            var status = batch.Status?.ToLower();
            if (status == "soldout" || status == "damaged" || status == "quarantined" || status == "reserved")
                return false;

            return status == "available" || status == "expired" || status == "active";
        }

        public async Task<IEnumerable<SmartPharmacySystem.Application.DTOs.Notifications.ExpiryAlertDto>> GetRealTimeExpiryAlertsAsync()
        {
            // Fetch batches with Medicine included
            var batches = await unitOfWork.MedicineBatches.GetAllWithMedicineAsync();

            var today = DateTime.UtcNow.Date;
            var alerts = new List<SmartPharmacySystem.Application.DTOs.Notifications.ExpiryAlertDto>();

            // Filter: Quantity > 0 and Active/Expiring within 60 days
            foreach (var batch in batches.Where(b => !b.IsDeleted && b.RemainingQuantity > 0))
            {
                var daysLeft = (batch.ExpiryDate.Date - today).Days;

                if (daysLeft <= 60)
                {
                    ExpiryAlertLevel level;
                    string levelText;
                    string colorCode;

                    if (daysLeft < 7) // Requirement: < 7 days (or <= 7? User said "< 7 days"). Usually means 6, 5... but "Critical" implies imminent. Let's strictly follow "< 7" or assume "Within 7 days" (<= 7).
                    {
                        // User said "< 7 days". Strict < 7 means 6 and below.
                        // User listed:
                        // < 7 days: Critical
                        // < 14 days: High
                        // < 30 days: Medium
                        // < 60 days: Normal

                        // Implementation Strategy:
                        // If daysLeft < 7 -> Critical
                        // Else if daysLeft < 14 -> High
                        // Else if daysLeft < 30 -> Medium
                        // Else if daysLeft < 60 -> Normal

                        // Note: If daysLeft is negative (expired), it should also be Critical.
                    }

                    if (daysLeft < 7)
                    {
                        level = ExpiryAlertLevel.Critical;
                        levelText = "خطر جداً";
                        colorCode = "#CC0000";
                    }
                    else if (daysLeft < 14)
                    {
                        level = ExpiryAlertLevel.High;
                        levelText = "تحذير عالٍ";
                        colorCode = "#FF8800";
                    }
                    else if (daysLeft < 30)
                    {
                        level = ExpiryAlertLevel.Medium;
                        levelText = "تنبيه متوسط";
                        colorCode = "#FFBB33";
                    }
                    else // < 60 (Implicitly covers 30 to 59)
                    {
                        level = ExpiryAlertLevel.Normal;
                        levelText = "تنبيه عادي";
                        colorCode = "#0099CC";
                    }

                    alerts.Add(new SmartPharmacySystem.Application.DTOs.Notifications.ExpiryAlertDto
                    {
                        MedicineName = batch.Medicine?.Name ?? "Unknown",
                        BatchNumber = batch.CompanyBatchNumber ?? "N/A",
                        ExpiryDate = batch.ExpiryDate,
                        DaysRemaining = daysLeft,
                        Quantity = batch.RemainingQuantity,
                        AlertLevel = level,
                        AlertLevelText = levelText,
                        ColorCode = colorCode
                    });
                }
            }

            // Order by DaysRemaining ascending (Nearest expiry first)
            return alerts.OrderBy(a => a.DaysRemaining).ToList();
        }
        public async Task<IEnumerable<UnifiedAlertDto>> GetActiveSystemAlertsAsync()
        {
            var alerts = new List<UnifiedAlertDto>();
            var today = DateTime.UtcNow.Date;

            // 1. Expiry Alerts (Batches with Quantity > 0)
            var batches = await unitOfWork.MedicineBatches.GetAllWithMedicineAsync();
            var activeBatches = batches.Where(b => !b.IsDeleted && b.RemainingQuantity > 0).ToList();

            foreach (var batch in activeBatches)
            {
                var daysLeft = (batch.ExpiryDate.Date - today).Days;

                if (daysLeft <= 60)
                {
                    string color;
                    string level;

                    if (daysLeft < 7)
                    {
                        color = "#CC0000"; // Critical
                        level = "Critical"; // خطر جداً
                    }
                    else if (daysLeft < 15)
                    {
                        color = "#FF8800"; // HighWarning
                        level = "HighWarning"; // تحذير عالٍ
                    }
                    else if (daysLeft < 30)
                    {
                        color = "#FFBB33"; // Warning
                        level = "Warning"; // تنبيه
                    }
                    else
                    {
                        color = "#0099CC"; // Info
                        level = "Info"; // معلومة
                    }

                    alerts.Add(new UnifiedAlertDto
                    {
                        Title = "تنبيه صلاحية",
                        Message = $"الدواء {batch.Medicine?.Name} (تشغيلة {batch.CompanyBatchNumber}) ينتهي خلال {daysLeft} يوم.",
                        StatusColor = color,
                        StatusLevel = level,
                        AlertType = "Expiry",
                        DaysRemaining = daysLeft,
                        Quantity = batch.RemainingQuantity,
                        BatchNumber = batch.CompanyBatchNumber ?? "N/A",
                        MedicineId = batch.MedicineId,
                        BatchId = batch.Id
                    });
                }
            }

            // 2. Low Stock Alerts (Medicines)
            var medicines = await unitOfWork.Medicines.GetAllAsync();
            foreach (var med in medicines.Where(m => !m.IsDeleted))
            {
                // Calculate current total stock
                var medBatches = batches.Where(b => b.MedicineId == med.Id && !b.IsDeleted); // Use fetched batches cache if possible, or query
                var totalStock = medBatches.Sum(b => b.RemainingQuantity);

                // Alert if Stock <= MinAlertQuantity OR Stock == 0
                if (totalStock <= med.MinAlertQuantity)
                {
                    string color;
                    string level;

                    if (totalStock == 0)
                    {
                        color = "#CC0000"; // Critical
                        level = "Critical";
                    }
                    else
                    {
                        color = "#FFBB33"; // Warning
                        level = "Warning";
                    }

                    alerts.Add(new UnifiedAlertDto
                    {
                        Title = "تنبيه نقص المخزون",
                        Message = $"رصيد الدواء {med.Name} منخفض ({totalStock} عبوة). الحد الأدنى: {med.MinAlertQuantity}",
                        StatusColor = color,
                        StatusLevel = level,
                        AlertType = "LowStock",
                        DaysRemaining = 0, // N/A
                        Quantity = totalStock,
                        BatchNumber = "ALL", // Aggregated
                        MedicineId = med.Id,
                        BatchId = 0
                    });
                }
            }

            // Sort: Critical first, then by days remaining (for expiry)
            return alerts.OrderBy(a => a.StatusLevel == "Critical" ? 0 : 1).ThenBy(a => a.DaysRemaining).ToList();
        }
    }
}
