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
            var expense = _mapper.Map<Expense>(dto);

            // Explicitly set CreatedBy to ensure it's not null
            expense.CreatedBy = dto.CreatedBy;

            expense.CreatedAt = DateTime.UtcNow;
            expense.IsDeleted = false;
            await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            // Financial Integration
            await _financialService.ProcessTransactionAsync(
                expense.Amount,
                FinancialTransactionType.Expense,
                $"General Expense: {expense.ExpenseType} - {expense.Notes ?? ""}"
            );

            return _mapper.Map<ExpenseDto>(expense);
        }

        public async Task UpdateExpenseAsync(int id, UpdateExpenseDto dto)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"المصروف برقم {id} غير موجود");

            _mapper.Map(dto, expense);

            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var exists = await _unitOfWork.Expenses.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"المصروف برقم {id} غير موجود");

            await _unitOfWork.Expenses.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense == null) return null;
            return _mapper.Map<ExpenseDto>(expense);
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
                query.ExpenseType);

            var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(items);
            return new PagedResult<ExpenseDto>(dtos, totalCount, query.Page, query.PageSize);
        }
    }
}
