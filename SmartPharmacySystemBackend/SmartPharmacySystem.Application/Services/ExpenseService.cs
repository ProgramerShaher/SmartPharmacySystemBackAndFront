using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IFinancialService _financialService;

        public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ExpenseService> logger, IFinancialService financialService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _financialService = financialService;
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto)
        {
            var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(dto.CategoryId)
                ?? throw new KeyNotFoundException("فئة المصروف غير موجودة");

            var expense = _mapper.Map<Expense>(dto);

            // Explicitly set CreatedBy to ensure it's not null
            expense.CreatedBy = dto.CreatedBy;
            expense.AccountId = dto.AccountId ?? 1; // Default to main account

            expense.CreatedAt = DateTime.UtcNow;
            expense.IsDeleted = false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Expenses.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();

                // Financial Integration - only for cash expenses
                if (expense.PaymentMethod == PaymentType.Cash)
                {
                    await _financialService.ProcessTransactionAsync(
                        accountId: expense.AccountId,
                        amount: expense.Amount,
                        type: FinancialTransactionType.Expense,
                        referenceType: ReferenceType.Expense,
                        referenceId: expense.Id,
                        description: $"مصروف: {category.Name} - {expense.Notes ?? ""}");

                    expense.IsPaid = true;
                    await _unitOfWork.Expenses.UpdateAsync(expense);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    expense.IsPaid = false;
                }

                await _unitOfWork.CommitAsync();

                var result = _mapper.Map<ExpenseDto>(expense);
                result.CategoryName = category.Name;
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateExpenseAsync(int id, UpdateExpenseDto dto)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المصروف برقم {id} غير موجود");

            var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(dto.CategoryId)
                ?? throw new KeyNotFoundException("فئة المصروف غير موجودة");

            decimal oldAmount = expense.Amount;
            var oldMethod = expense.PaymentMethod;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, expense);

                // Professional Sync: Sync IsPaid with PaymentMethod
                expense.IsPaid = expense.PaymentMethod == PaymentType.Cash;

                // 1. Handling Payment Method Transitions
                if (oldMethod == PaymentType.Cash && expense.PaymentMethod == PaymentType.Credit)
                {
                    // Switched from Cash to Credit: Reverse original vault transaction
                    await _financialService.ReverseFinancialTransactionAsync(
                        ReferenceType.Expense, expense.Id,
                        $"تغيير طريقة الدفع من نقدي إلى آجل - استرداد مبلغ: {oldAmount}");
                }
                else if (oldMethod == PaymentType.Credit && expense.PaymentMethod == PaymentType.Cash)
                {
                    // Switched from Credit to Cash: Create new vault transaction
                    await _financialService.ProcessTransactionAsync(
                        expense.AccountId, expense.Amount, FinancialTransactionType.Expense,
                        ReferenceType.Expense, expense.Id,
                        $"تغيير طريقة الدفع إلى نقدي: {category.Name}");
                }
                // 2. Handling Amount Changes for existing Cash expenses
                else if (expense.PaymentMethod == PaymentType.Cash && oldAmount != expense.Amount)
                {
                    decimal difference = expense.Amount - oldAmount;
                    var correctionType = difference > 0
                        ? FinancialTransactionType.Expense
                        : FinancialTransactionType.Income;

                    await _financialService.ProcessTransactionAsync(
                        accountId: expense.AccountId,
                        amount: Math.Abs(difference),
                        type: correctionType,
                        referenceType: ReferenceType.Expense,
                        referenceId: expense.Id,
                        description: $"تعديل مبلغ مصروف {category.Name} من {oldAmount} إلى {expense.Amount}");
                }

                await _unitOfWork.Expenses.UpdateAsync(expense);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "خطأ أثناء تحديث المصروف {Id}", id);
                throw;
            }
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المصروف برقم {id} غير موجود");

            var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(expense.CategoryId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // إذا تم دفع المصروف نقداً، يجب عكس الحركة المالية عند الحذف
                if (expense.IsPaid && expense.PaymentMethod == PaymentType.Cash)
                {
                    await _financialService.ReverseFinancialTransactionAsync(
                        ReferenceType.Expense,
                        expense.Id,
                        $"حذف مصروف: {category?.Name ?? ""} - استرداد المبلغ للخزينة");
                }

                await _unitOfWork.Expenses.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("تم حذف المصروف {Id} وعكس حركته المالية بنجاح", id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "خطأ أثناء حذف المصروف {Id}", id);
                throw;
            }
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            return expense == null ? null : _mapper.Map<ExpenseDto>(expense);
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync()
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
        }

        public async Task<PagedResult<ExpenseDto>> SearchAsync(ExpenseQueryDto query)
        {
            var (items, totalCount) = await _unitOfWork.Expenses.GetPagedAsync(
                query.Search,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDirection,
                query.FromDate,
                query.ToDate,
                query.CategoryId);

            var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(items);
            return new PagedResult<ExpenseDto>(dtos, totalCount, query.Page, query.PageSize);
        }

        // Category Methods
        public async Task<IEnumerable<ExpenseCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.ExpenseCategories.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseCategoryDto>>(categories);
        }

        public async Task<ExpenseCategoryDto> CreateCategoryAsync(CreateExpenseCategoryDto dto)
        {
            var category = _mapper.Map<ExpenseCategory>(dto);
            await _unitOfWork.ExpenseCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ExpenseCategoryDto>(category);
        }

        public async Task UpdateCategoryAsync(int id, UpdateExpenseCategoryDto dto)
        {
            var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("الفئة غير موجودة");

            _mapper.Map(dto, category);
            await _unitOfWork.ExpenseCategories.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _unitOfWork.ExpenseCategories.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
