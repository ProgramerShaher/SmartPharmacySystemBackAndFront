using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.MedicineBatch;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;


namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Controller for managing medicine batches (Lots) in the pharmacy system.
/// متحكم لإدارة دفعات الأدوية (اللوطات) في نظام الصيدلية.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicineBatchesController : ControllerBase
{
    private readonly IMedicineBatchService _service;

    public MedicineBatchesController(IMedicineBatchService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets the current authenticated user ID.
    /// يحصل على معرف المستخدم الحالي المصادق عليه.
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            return parsedId;
        return 1; // Default to system/admin if not authenticated
    }

    // ===================== CRUD Operations =====================

    /// <summary>
    /// Creates a new medicine batch.
    /// إنشاء دفعة دواء جديدة.
    /// </summary>
    /// <remarks>
    /// Creates a new batch for a medicine with all required details including quantity, expiry date, and pricing.
    /// يقوم بإنشاء دفعة جديدة لدواء مع جميع التفاصيل المطلوبة بما في ذلك الكمية وتاريخ انتهاء الصلاحية والتسعير.
    /// </remarks>
    /// <param name="dto">Batch creation data | بيانات إنشاء الدفعة</param>
    /// <returns>The created batch | الدفعة التي تم إنشاؤها</returns>
    /// <response code="201">Batch created successfully | تم إنشاء الدفعة بنجاح</response>
    /// <response code="400">Invalid input data | بيانات الإدخال غير صالحة</response>
    /// <response code="409">Duplicate batch number or barcode | رقم الدفعة أو الباركود مكرر</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] MedicineBatchCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Failed("Invalid input data | بيانات الإدخال غير صالحة"));

        dto.CreatedBy = GetCurrentUserId();

        var result = await _service.CreateBatchAsync(dto);
        return StatusCode(201, ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            "Batch created successfully | تم إنشاء الدفعة بنجاح", 201));
    }

    /// <summary>
    /// Updates an existing medicine batch.
    /// تحديث دفعة دواء موجودة.
    /// </summary>
    /// <remarks>
    /// Updates batch details like quantity, pricing, storage location, or status.
    /// يقوم بتحديث تفاصيل الدفعة مثل الكمية والتسعير وموقع التخزين أو الحالة.
    /// </remarks>
    /// <param name="id">Batch ID | معرف الدفعة</param>
    /// <param name="dto">Updated batch data | بيانات الدفعة المحدثة</param>
    /// <returns>The updated batch | الدفعة المحدثة</returns>
    /// <response code="200">Batch updated successfully | تم تحديث الدفعة بنجاح</response>
    /// <response code="400">Invalid input data | بيانات الإدخال غير صالحة</response>
    /// <response code="404">Batch not found | الدفعة غير موجودة</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MedicineBatchUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Failed("Invalid input data | بيانات الإدخال غير صالحة"));



        var result = await _service.UpdateBatchAsync(id, dto);
        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            "Batch updated successfully | تم تحديث الدفعة بنجاح"));
    }

    /// <summary>
    /// Deletes a medicine batch (soft delete).
    /// حذف دفعة دواء (حذف ناعم).
    /// </summary>
    /// <remarks>
    /// Performs a soft delete on the batch, marking it as deleted without permanent removal.
    /// ينفذ حذفاً ناعماً على الدفعة، ويضع علامة عليها كمحذوفة بدون إزالة دائمة.
    /// </remarks>
    /// <param name="id">Batch ID | معرف الدفعة</param>
    /// <response code="200">Batch deleted successfully | تم حذف الدفعة بنجاح</response>
    /// <response code="404">Batch not found | الدفعة غير موجودة</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteBatchAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.Succeeded(null, "Batch deleted successfully | تم حذف الدفعة بنجاح"));
    }

    // ===================== Query Operations =====================

    /// <summary>
    /// Gets all medicine batches with optional search filter.
    /// جلب جميع دفعات الأدوية مع خيار البحث.
    /// </summary>
    /// <remarks>
    /// Returns all non-deleted batches. Optional filter searches by batch number, barcode, medicine name, or storage location.
    /// يرجع جميع الدفعات غير المحذوفة. الفلتر الاختياري يبحث حسب رقم الدفعة، الباركود، اسم الدواء، أو موقع التخزين.
    /// </remarks>
    /// <param name="filter">Optional search filter | فلتر البحث الاختياري</param>
    /// <returns>List of batches | قائمة الدفعات</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? filter = null)
    {
        var result = await _service.GetAllBatchesAsync(filter);
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            "Batches retrieved successfully | تم جلب الدفعات بنجاح"));
    }

    /// <summary>
    /// Gets a specific batch by ID.
    /// جلب دفعة محددة بالمعرف.
    /// </summary>
    /// <param name="id">Batch ID | معرف الدفعة</param>
    /// <returns>The batch details | تفاصيل الدفعة</returns>
    /// <response code="200">Batch found | تم العثور على الدفعة</response>
    /// <response code="404">Batch not found | الدفعة غير موجودة</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetBatchByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Failed("Batch not found | الدفعة غير موجودة", 404));

        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            "Batch retrieved successfully | تم جلب الدفعة بنجاح"));
    }

    /// <summary>
    /// Gets a batch by its barcode.
    /// جلب دفعة بالباركود.
    /// </summary>
    /// <remarks>
    /// Useful for barcode scanning at point of sale.
    /// مفيد لمسح الباركود عند نقطة البيع.
    /// </remarks>
    /// <param name="barcode">Batch barcode | باركود الدفعة</param>
    /// <returns>The batch details | تفاصيل الدفعة</returns>
    /// <response code="200">Batch found | تم العثور على الدفعة</response>
    /// <response code="404">Batch not found | الدفعة غير موجودة</response>
    [HttpGet("barcode/{barcode}")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBarcode(string barcode)
    {
        var result = await _service.GetBatchByBarcodeAsync(barcode);
        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            "Batch retrieved successfully | تم جلب الدفعة بنجاح"));
    }

    /// <summary>
    /// Gets all batches for a specific medicine.
    /// جلب جميع الدفعات لدواء محدد.
    /// </summary>
    /// <param name="medicineId">Medicine ID | معرف الدواء</param>
    /// <returns>List of batches for the medicine | قائمة دفعات الدواء</returns>
    [HttpGet("medicine/{medicineId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMedicineId(int medicineId)
    {
        var result = await _service.GetBatchesByMedicineIdAsync(medicineId);
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            "Batches retrieved successfully | تم جلب الدفعات بنجاح"));
    }

    /// <summary>
    /// Gets all available batches (can be sold).
    /// جلب جميع الدفعات المتاحة (يمكن بيعها).
    /// </summary>
    /// <remarks>
    /// Returns batches with Available status, positive quantity, and not expired.
    /// يرجع الدفعات ذات الحالة المتاحة والكمية الموجبة وغير المنتهية الصلاحية.
    /// </remarks>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _service.GetAvailableBatchesAsync();
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            "Available batches retrieved successfully | تم جلب الدفعات المتاحة بنجاح"));
    }

    /// <summary>
    /// Gets available batches for a specific medicine (FIFO ordered).
    /// جلب الدفعات المتاحة لدواء محدد (مرتبة بالأول أولاً).
    /// </summary>
    /// <remarks>
    /// Returns available batches ordered by expiry date (nearest first) for FIFO selling.
    /// يرجع الدفعات المتاحة مرتبة حسب تاريخ انتهاء الصلاحية (الأقرب أولاً) للبيع بمنطق الأول أولاً.
    /// </remarks>
    /// <param name="medicineId">Medicine ID | معرف الدواء</param>
    [HttpGet("medicine/{medicineId:int}/available")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableByMedicineId(int medicineId)
    {
        var result = await _service.GetAvailableBatchesByMedicineIdAsync(medicineId);
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            "Available batches retrieved successfully | تم جلب الدفعات المتاحة بنجاح"));
    }

    /// <summary>
    /// Gets batches expiring soon.
    /// جلب الدفعات التي ستنتهي قريباً.
    /// </summary>
    /// <remarks>
    /// Returns batches expiring within the specified number of days (default 60).
    /// يرجع الدفعات التي ستنتهي خلال عدد الأيام المحدد (الافتراضي 60).
    /// </remarks>
    /// <param name="days">Days threshold | عدد الأيام</param>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiring([FromQuery] int days = 60)
    {
        var result = await _service.GetExpiringBatchesAsync(days);
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            $"Batches expiring within {days} days | الدفعات المنتهية خلال {days} يوماً"));
    }

    /// <summary>
    /// Gets already expired batches.
    /// جلب الدفعات المنتهية الصلاحية.
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpired()
    {
        var result = await _service.GetExpiredBatchesAsync();
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            "Expired batches retrieved successfully | تم جلب الدفعات المنتهية الصلاحية"));
    }

    /// <summary>
    /// Gets batches by status.
    /// جلب الدفعات حسب الحالة.
    /// </summary>
    /// <param name="status">Batch status | حالة الدفعة</param>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicineBatchResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var result = await _service.GetBatchesByStatusAsync(status);
        return Ok(ApiResponse<IEnumerable<MedicineBatchResponseDto>>.Succeeded(result, 
            $"Batches with status {status} retrieved | تم جلب الدفعات بالحالة {status}"));
    }

    /// <summary>
    /// Gets total available quantity for a medicine.
    /// جلب إجمالي الكمية المتاحة لدواء.
    /// </summary>
    /// <param name="medicineId">Medicine ID | معرف الدواء</param>
    [HttpGet("medicine/{medicineId:int}/quantity")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalQuantity(int medicineId)
    {
        var quantity = await _service.GetTotalAvailableQuantityAsync(medicineId);
        return Ok(ApiResponse<int>.Succeeded(quantity, 
            $"Total available quantity: {quantity} | إجمالي الكمية المتاحة: {quantity}"));
    }

    // ===================== Business Operations =====================

    /// <summary>
    /// Sell from batches using FIFO logic.
    /// البيع من الدفعات باستخدام منطق الأول أولاً.
    /// </summary>
    /// <remarks>
    /// Automatically sells from batches with the nearest expiry date first.
    /// يبيع تلقائياً من الدفعات ذات أقرب تاريخ انتهاء صلاحية أولاً.
    /// </remarks>
    /// <param name="medicineId">Medicine ID | معرف الدواء</param>
    /// <param name="quantity">Quantity to sell | الكمية للبيع</param>
    /// <response code="200">Sale completed | تم البيع بنجاح</response>
    /// <response code="400">Insufficient quantity or invalid request | الكمية غير كافية أو طلب غير صالح</response>
    [HttpPost("sell/fifo")]
    [ProducesResponseType(typeof(ApiResponse<BatchSaleResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SellFIFO([FromQuery] int medicineId, [FromQuery] int quantity)
    {
        var result = await _service.SellFromBatchFIFOAsync(medicineId, quantity, GetCurrentUserId());
        
        if (!result.Success)
            return BadRequest(ApiResponse<BatchSaleResultDto>.Failed(result.Message));

        return Ok(ApiResponse<BatchSaleResultDto>.Succeeded(result, result.Message));
    }

    /// <summary>
    /// Sell from a specific batch.
    /// البيع من دفعة محددة.
    /// </summary>
    /// <param name="batchId">Batch ID | معرف الدفعة</param>
    /// <param name="quantity">Quantity to sell | الكمية للبيع</param>
    /// <response code="200">Sale completed | تم البيع بنجاح</response>
    /// <response code="400">Cannot sell from this batch | لا يمكن البيع من هذه الدفعة</response>
    [HttpPost("{batchId:int}/sell")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SellFromBatch(int batchId, [FromQuery] int quantity)
    {
        var result = await _service.SellFromBatchAsync(batchId, quantity, GetCurrentUserId());
        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            $"Sold {quantity} units successfully | تم بيع {quantity} وحدة بنجاح"));
    }

    /// <summary>
    /// Return quantity to a batch (for sales returns).
    /// إرجاع الكمية إلى دفعة (لمرتجعات المبيعات).
    /// </summary>
    /// <param name="batchId">Batch ID | معرف الدفعة</param>
    /// <param name="quantity">Quantity to return | الكمية للإرجاع</param>
    [HttpPost("{batchId:int}/return")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReturnToBatch(int batchId, [FromQuery] int quantity)
    {
        var result = await _service.ReturnToBatchAsync(batchId, quantity, GetCurrentUserId());
        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            $"Returned {quantity} units successfully | تم إرجاع {quantity} وحدة بنجاح"));
    }

    /// <summary>
    /// Marks a batch as damaged.
    /// وضع علامة على دفعة كتالفة.
    /// </summary>
    /// <param name="batchId">Batch ID | معرف الدفعة</param>
    /// <param name="reason">Reason for damage | سبب التلف</param>
    [HttpPost("{batchId:int}/damage")]
    [ProducesResponseType(typeof(ApiResponse<MedicineBatchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDamaged(int batchId, [FromQuery] string? reason = null)
    {
        var result = await _service.MarkBatchAsDamagedAsync(batchId, reason, GetCurrentUserId());
        return Ok(ApiResponse<MedicineBatchResponseDto>.Succeeded(result, 
            "Batch marked as damaged | تم وضع علامة على الدفعة كتالفة"));
    }

    /// <summary>
    /// Updates all expired batches to Expired status.
    /// تحديث جميع الدفعات المنتهية الصلاحية إلى حالة منتهية.
    /// </summary>
    /// <remarks>
    /// Should be called periodically (e.g., daily job) to automatically mark expired batches.
    /// يجب استدعاؤها بشكل دوري (مثلاً مهمة يومية) لوضع علامة تلقائية على الدفعات المنتهية.
    /// </remarks>
    /// <returns>Number of batches updated | عدد الدفعات المحدثة</returns>
    [HttpPost("update-expired")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateExpiredBatches()
    {
        var count = await _service.UpdateExpiredBatchesAsync();
        return Ok(ApiResponse<int>.Succeeded(count, 
            $"Updated {count} expired batches | تم تحديث {count} دفعة منتهية الصلاحية"));
    }

    /// <summary>
    /// Validates if a batch can be sold from.
    /// التحقق مما إذا كان يمكن البيع من دفعة.
    /// </summary>
    /// <param name="batchId">Batch ID | معرف الدفعة</param>
    /// <param name="quantity">Quantity to sell | الكمية للبيع</param>
    [HttpGet("{batchId:int}/validate-sale")]
    [ProducesResponseType(typeof(ApiResponse<BatchValidationResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateForSale(int batchId, [FromQuery] int quantity)
    {
        var result = await _service.ValidateBatchForSaleAsync(batchId, quantity);
        return Ok(ApiResponse<BatchValidationResultDto>.Succeeded(result, 
            result.IsValid ? "Batch is valid for sale | الدفعة صالحة للبيع" : "Batch is not valid for sale | الدفعة غير صالحة للبيع"));
    }
}
