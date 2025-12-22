using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto);
        Task UpdateExpenseAsync(int id, UpdateExpenseDto dto);
        Task DeleteExpenseAsync(int id);
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync();
        Task<PagedResult<ExpenseDto>> SearchAsync(ExpenseQueryDto query);
    }
}
