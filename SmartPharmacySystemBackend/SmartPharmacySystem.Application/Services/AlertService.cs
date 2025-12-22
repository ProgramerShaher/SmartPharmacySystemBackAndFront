// SmartPharmacySystem.Application/Services/AlertService.cs

using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.DTOs.Alerts;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Application.Helpers;

namespace SmartPharmacySystem.Application.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IMedicineBatchRepository _batchRepository;
        private readonly ILogger<AlertService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AlertService(
            IAlertRepository alertRepository,
            IMedicineBatchRepository batchRepository,
            ILogger<AlertService> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _alertRepository = alertRepository;
            _batchRepository = batchRepository;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        // -------------------------------
        // Get All
        // -------------------------------
        public async Task<IEnumerable<AlertDto>> GetAllAsync()
        {
            _logger.LogInformation("جلب جميع التنبيهات");
            var alerts = await _alertRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        // -------------------------------
        // Get By ID
        // -------------------------------
        public async Task<AlertDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("جلب التنبيه برقم {Id}", id);

            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null)
            {
                _logger.LogWarning("التنبيه برقم {Id} غير موجود", id);
                throw new KeyNotFoundException($"التنبيه برقم {id} غير موجود");
            }

            return _mapper.Map<AlertDto>(alert);
        }

        // -------------------------------
        // Create
        // -------------------------------
        public async Task<AlertDto> CreateAsync(CreateAlertDto dto)
        {
            _logger.LogInformation("إنشاء تنبيه جديد لدفعة {BatchId}", dto.BatchId);

            // التحقق من وجود الدفعة
            var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
            if (batch == null)
            {
                _logger.LogWarning("الدفعة برقم {BatchId} غير موجودة", dto.BatchId);
                throw new KeyNotFoundException($"الدفعة برقم {dto.BatchId} غير موجودة");
            }

            var alert = _mapper.Map<Alert>(dto);
            alert.Status = AlertStatus.Pending;
            alert.CreatedAt = DateTime.UtcNow;
            alert.IsDeleted = false;

            await _alertRepository.AddAsync(alert);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("تم إنشاء التنبيه بنجاح برقم {Id}", alert.Id);

            // Assign the batch to the alert to ensure it's available for mapping
            alert.Batch = batch;

            return _mapper.Map<AlertDto>(alert);
        }

        public async Task<AlertDto> UpdateAsync(int id, UpdateAlertDto dto)
        {
            _logger.LogInformation("تحديث التنبيه برقم {Id}", id);

            if (id != dto.Id)
                throw new ArgumentException("رقم التنبيه غير متطابق");

            var existing = await _alertRepository.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("التنبيه برقم {Id} غير موجود", id);
                throw new KeyNotFoundException($"التنبيه برقم {id} غير موجود");
            }

            _mapper.Map(dto, existing);

            await _alertRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("تم تحديث التنبيه برقم {Id} بنجاح", id);

            return _mapper.Map<AlertDto>(existing);
        }

        // -------------------------------
        // Delete
        // -------------------------------
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("حذف التنبيه برقم {Id}", id);

            var existing = await _alertRepository.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("التنبيه برقم {Id} غير موجود", id);
                throw new KeyNotFoundException($"التنبيه برقم {id} غير موجود");
            }

            await _alertRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("تم حذف التنبيه برقم {Id} بنجاح", id);

            return true;
        }

        // -------------------------------
        // Get Alerts by Batch ID
        // -------------------------------
        public async Task<IEnumerable<AlertDto>> GetByBatchIdAsync(int batchId)
        {
            _logger.LogInformation("جلب تنبيهات الدفعة {BatchId}", batchId);

            var alerts = await _alertRepository.GetByBatchIdAsync(batchId);
            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        // -------------------------------
        // Get Alerts by Status
        // -------------------------------
        public async Task<IEnumerable<AlertDto>> GetByStatusAsync(AlertStatus status)
        {
            _logger.LogInformation("جلب التنبيهات بالحالة {Status}", status);

            var alerts = await _alertRepository.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
        }

        // -------------------------------
        // Mark as Read
        // -------------------------------
        public async Task MarkAsReadAsync(int id)
        {
            _logger.LogInformation("تحديد التنبيه برقم {Id} كمقروء", id);

            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null)
            {
                _logger.LogWarning("التنبيه برقم {Id} غير موجود", id);
                throw new KeyNotFoundException($"التنبيه برقم {id} غير موجود");
            }

            alert.Status = AlertStatus.Read;

            await _alertRepository.UpdateAsync(alert);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("تم تحديد التنبيه برقم {Id} كمقروء", id);
        }

        // -------------------------------
        // Auto-Generate Expiry Alerts
        // -------------------------------
        public async Task GenerateExpiryAlertsAsync()
        {
            _logger.LogInformation("بدء إنشاء تنبيهات الانتهاء التلقائية");

            var today = DateTime.UtcNow;
            var batches = await _batchRepository.GetExpiringBatchesAsync();

            foreach (var batch in batches)
            {
                // Validate batch status - skip invalid batches
                if (!IsBatchEligibleForAlert(batch))
                {
                    _logger.LogDebug("تخطي الدفعة {BatchId} - الحالة: {Status}", batch.Id, batch.Status);
                    continue;
                }

                var timeLeft = batch.ExpiryDate - today;

                // Determine AlertType based on ExpiryStatus mapping
                AlertType? alertType = timeLeft.TotalDays switch
                {
                    <= 7 => AlertType.ExpiryOneWeek,
                    <= 14 => AlertType.ExpiryTwoWeeks,
                    <= 30 => AlertType.ExpiryOneMonth,
                    <= 60 => AlertType.ExpiryTwoMonths,
                    _ => null
                };

                // Check if batch is already expired
                if (batch.ExpiryDate.Date <= today.Date)
                {
                    alertType = AlertType.Expired;
                }

                if (alertType == null)
                    continue;

                // Prevent duplicate alerts
                var existingAlerts = await _alertRepository.GetByBatchIdAsync(batch.Id);
                if (existingAlerts.Any(a => a.AlertType == alertType.Value && a.ExecutionDate.Date == today.Date))
                {
                    _logger.LogDebug("تخطي الدفعة {BatchId} - التنبيه موجود بالفعل", batch.Id);
                    continue;
                }

                var alert = new Alert
                {
                    BatchId = batch.Id,
                    AlertType = alertType.Value,
                    ExecutionDate = today,
                    Status = AlertStatus.Pending,
                    IsDeleted = false,
                    CreatedAt = today
                };

                await _alertRepository.AddAsync(alert);

                _logger.LogInformation("تم إنشاء تنبيه {AlertType} للدفعة {BatchId}", alertType, batch.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("تم إنشاء جميع تنبيهات الانتهاء بنجاح");
        }

        /// <summary>
        /// Validates if a batch is eligible for alert generation.
        /// Skips SoldOut, Damaged, Quarantined, and Reserved batches.
        /// </summary>
        private bool IsBatchEligibleForAlert(MedicineBatch batch)
        {
            // Parse batch status string to enum (assuming Status is still string in MedicineBatch)
            var status = batch.Status?.ToLower();

            // Skip batches that should not generate alerts
            if (status == "soldout" || status == "damaged" ||
                status == "quarantined" || status == "reserved")
            {
                return false;
            }

            // Only generate alerts for Available and Expired batches
            return status == "available" || status == "expired" || status == "active";
        }
    }
}
