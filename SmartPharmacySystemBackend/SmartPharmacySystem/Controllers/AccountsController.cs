using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// جلب شجرة الحسابات بالكامل
        /// </summary>
        /// <access>Admin | Accountant</access>
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            try
            {
                var tree = await _accountService.GetAccountTreeAsync();
                return Ok(ApiResponse<IEnumerable<AccountDto>>.Succeeded(tree, "تم جلب شجرة الحسابات بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account tree");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب شجرة الحسابات"));
            }
        }

        /// <summary>
        /// جلب حساب معين بواسطة المعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var account = await _accountService.GetByIdAsync(id);
                return Ok(ApiResponse<AccountDto>.Succeeded(account, "تم جلب بيانات الحساب بنجاح"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب بيانات الحساب"));
            }
        }

        /// <summary>
        /// إضافة حساب جديد في شجرة الحسابات
        /// </summary>
        /// <access>Admin | Accountant</access>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات الحساب غير صالحة"));

            try
            {
                var created = await _accountService.CreateAsync(dto);
                return StatusCode(201, ApiResponse<AccountDto>.Succeeded(created, "تم إنشاء الحساب بنجاح", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء إنشاء الحساب"));
            }
        }

        /// <summary>
        /// تحديث بيانات حساب موجود
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AccountDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("معرف الحساب غير متطابق"));

            try
            {
                await _accountService.UpdateAsync(dto);
                return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث الحساب بنجاح"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تحديث الحساب"));
            }
        }

        /// <summary>
        /// تغيير حالة تفعيل الحساب (نشط/غير نشط)
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                await _accountService.ToggleStatusAsync(id, isActive);
                var statusMsg = isActive ? "مفعل" : "غير مفعل";
                return Ok(ApiResponse<object>.Succeeded(null, $"تم تغيير حالة الحساب إلى {statusMsg} بنجاح"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تغيير حالة الحساب"));
            }
        }

        /// <summary>
        /// حذف حساب من الشجرة
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _accountService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Succeeded(null, "تم حذف الحساب بنجاح"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء حذف الحساب"));
            }
        }

        /// <summary>
        /// جلب الرصيد الحالي لحساب معين
        /// </summary>
        [HttpGet("{id}/balance")]
        public async Task<IActionResult> GetBalance(int id)
        {
            try
            {
                var balance = await _accountService.GetBalanceAsync(id);
                return Ok(ApiResponse<decimal>.Succeeded(balance, "تم جلب الرصيد بنجاح"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance for account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب الرصيد"));
            }
        }
        /// <summary>
        /// جلب دفتر الأستاذ (كشف حساب تفصيلي) لحساب معين
        /// </summary>
        /// <param name="id">معرف الحساب</param>
        /// <param name="startDate">تاريخ البداية</param>
        /// <param name="endDate">تاريخ النهاية</param>
        [HttpGet("{id}/ledger")]
        public async Task<IActionResult> GetLedger(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var ledger = await _accountService.GetGeneralLedgerAsync(id, startDate, endDate);
                return Ok(ApiResponse<LedgerReportDto>.Succeeded(ledger, "تم جلب كشف الحساب بنجاح"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ledger for account {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب كشف الحساب"));
            }
        }

        /// <summary>
        /// جلب ميزان المراجعة لجميع الحسابات
        /// </summary>
        /// <param name="asOfDate">التاريخ المطلوب (اختياري)</param>
        [HttpGet("trial-balance")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime? asOfDate)
        {
            try
            {
                var trialBalance = await _accountService.GetTrialBalanceAsync(asOfDate);
                return Ok(ApiResponse<TrialBalanceDto>.Succeeded(trialBalance, "تم جلب ميزان المراجعة بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trial balance");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب ميزان المراجعة"));
            }
        }

        /// <summary>
        /// جلب قائمة الدخل
        /// </summary>
        [HttpGet("income-statement")]
        public async Task<IActionResult> GetIncomeStatement([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _accountService.GetIncomeStatementAsync(startDate, endDate);
                return Ok(ApiResponse<IncomeStatementDto>.Succeeded(report, "تم جلب قائمة الدخل بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching income statement");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب قائمة الدخل"));
            }
        }

        /// <summary>
        /// جلب الميزانية العمومية
        /// </summary>
        [HttpGet("balance-sheet")]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
        {
            try
            {
                var report = await _accountService.GetBalanceSheetAsync(asOfDate);
                return Ok(ApiResponse<BalanceSheetDto>.Succeeded(report, "تم جلب الميزانية العمومية بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance sheet");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب الميزانية العمومية"));
            }
        }

        /// <summary>
        /// جلب دفتر الأستاذ لجميع الحسابات التي تمت عليها حركات
        /// </summary>
        [HttpGet("all-ledgers")]
        public async Task<IActionResult> GetAllLedgers([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;
                var ledgers = await _accountService.GetAllLedgersAsync(start, end);
                return Ok(ApiResponse<IEnumerable<LedgerReportDto>>.Succeeded(ledgers, "تم جلب جميع كشوفات الحسابات بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all ledgers");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب كشوفات الحسابات"));
            }
        }
    }
}
