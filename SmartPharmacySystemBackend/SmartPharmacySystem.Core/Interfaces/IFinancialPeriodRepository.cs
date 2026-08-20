using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IFinancialPeriodRepository
{
    Task<FinancialPeriod> GetByIdAsync(int id);
    Task<IEnumerable<FinancialPeriod>> GetAllAsync();
    Task<FinancialPeriod> GetByDateAsync(System.DateTime date);
    Task AddAsync(FinancialPeriod entity);
    Task UpdateAsync(FinancialPeriod entity);
    Task DeleteAsync(int id);
}
